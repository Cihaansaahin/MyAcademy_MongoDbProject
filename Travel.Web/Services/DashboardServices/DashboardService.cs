using MongoDB.Bson;
using MongoDB.Driver;
using System.Globalization;
using Travel.Web.DTOs.DashboardDtos;
using Travel.Web.Entities;
using Travel.Web.Settings;

namespace Travel.Web.Services.DashboardServices
{
    public class DashboardService : IDashboardService
    {
        private readonly IMongoCollection<Tour> _tourCollection;
        private readonly IMongoCollection<Reservation> _reservationCollection;
        private readonly IMongoCollection<Question> _questionCollection;
        private readonly IMongoCollection<Category> _categoryCollection;
        private readonly IMongoCollection<Comment> _commentCollection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Destination> _destinationCollection;
        private readonly string _tourCollectionName;
        private readonly string _destinationCollectionName;

        // Rezervasyon tarihleri UTC saklanıyor; gün/ay gruplamasını Türkiye saatine göre yapıyoruz
        private const string TimeZone = "Europe/Istanbul";

        public DashboardService(IDataBaseSettings databaseSettings)
        {
            var client = new MongoClient(databaseSettings.ConnectionString);
            var database = client.GetDatabase(databaseSettings.DataBaseName);

            _tourCollection = database.GetCollection<Tour>(databaseSettings.TourCollectionName);
            _reservationCollection = database.GetCollection<Reservation>(databaseSettings.ReservationCollectionName);
            _questionCollection = database.GetCollection<Question>(databaseSettings.QuestionCollectionName);
            _categoryCollection = database.GetCollection<Category>(databaseSettings.CategoryCollectionName);
            _commentCollection = database.GetCollection<Comment>(databaseSettings.CommentCollectionName);
            _userCollection = database.GetCollection<User>(databaseSettings.UserCollectionName);
            _destinationCollection = database.GetCollection<Destination>(databaseSettings.DestinationCollectionName);

            _tourCollectionName = databaseSettings.TourCollectionName;
            _destinationCollectionName = databaseSettings.DestinationCollectionName;
        }

        public async Task<DashboardStatisticsDto> GetDashboardStatisticsAsync()
        {
            var dto = new DashboardStatisticsDto();

            // 1. Sayaçlar
            dto.TotalTours = await _tourCollection.CountDocumentsAsync(_ => true);
            dto.ActiveTours = await _tourCollection.CountDocumentsAsync(x => x.IsActive);
            dto.PassiveTours = dto.TotalTours - dto.ActiveTours;
            dto.TotalReservations = await _reservationCollection.CountDocumentsAsync(_ => true);

            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            dto.ThisMonthReservationsCount = await _reservationCollection.CountDocumentsAsync(x => x.ReservationDate >= startOfMonth);

            dto.TotalUsers = await _userCollection.CountDocumentsAsync(x => x.Role == "Member");
            dto.TotalDestinations = await _destinationCollection.CountDocumentsAsync(_ => true);

            dto.PendingQuestionsCount = await _questionCollection.CountDocumentsAsync(x => x.AnswerText == null || x.AnswerText == "");

            // 2. Tur bazlı satış raporu: en çok rezerve edilen 5 tur + en çok gelir getiren tur
            var tourStats = await GetTourSalesAsync();
            dto.Top5SellingTours = tourStats
                .OrderByDescending(x => x.TotalBookings)
                .ThenByDescending(x => x.TotalParticipants)
                .Take(5)
                .ToList();
            dto.TopBookedTourName = dto.Top5SellingTours.FirstOrDefault()?.TourTitle ?? string.Empty;
            dto.TopRevenueTour = tourStats.OrderByDescending(x => x.TotalRevenue).FirstOrDefault();
            dto.TotalRevenue = tourStats.Sum(x => x.TotalRevenue);

            // 3. Destinasyon bazlı rezervasyon sayısı ($lookup ile Tours -> Destinations)
            dto.ReservationsByDestination = await GetReservationsByDestinationAsync();

            // 4. Ortalama puan (onaylı yorumlar)
            var ratingResult = await _commentCollection.Aggregate()
                .Match(x => x.IsApproved)
                .Group(x => x.IsApproved, g => new { Average = g.Average(c => c.Rating), Count = g.Count() })
                .FirstOrDefaultAsync();
            if (ratingResult != null)
            {
                dto.AverageRating = Math.Round(ratingResult.Average, 1);
                dto.TotalReviews = ratingResult.Count;
            }

            // 5. Kategori bazında tur sayısı
            var rawCategoryCounts = await _tourCollection.Aggregate()
                .Group(x => x.CategoryId, g => new { CategoryId = g.Key, Count = g.Count() })
                .ToListAsync();
            var categories = await _categoryCollection.Find(_ => true).ToListAsync();

            foreach (var item in rawCategoryCounts)
            {
                var category = categories.FirstOrDefault(c => c.Id == item.CategoryId);
                dto.ToursByCategory.Add(new CategoryTourCountDto
                {
                    CategoryId = item.CategoryId,
                    CategoryName = category?.Name?.ToString() ?? "-",
                    TourCount = item.Count
                });
            }

            // 6. Grafikler: Son 6 ay (aylık) ve son 30 gün (günlük) — gruplama MongoDB'de yapılıyor
            var culture = CultureInfo.CurrentUICulture;
            var nowLocal = GetLocalNow();

            var monthStart = new DateTime(nowLocal.Year, nowLocal.Month, 1).AddMonths(-5);
            var monthly = await CountReservationsByDateAsync(monthStart.AddDays(-1).ToUniversalTime(), "%Y-%m");
            for (int i = 0; i < 6; i++)
            {
                var m = monthStart.AddMonths(i);
                dto.MonthlyChartLabels.Add(culture.TextInfo.ToTitleCase(m.ToString("MMMM yyyy", culture)));
                dto.MonthlyChartData.Add(monthly.TryGetValue(m.ToString("yyyy-MM"), out var c) ? c : 0);
            }

            var dayStart = nowLocal.Date.AddDays(-29);
            var daily = await CountReservationsByDateAsync(dayStart.AddDays(-1).ToUniversalTime(), "%Y-%m-%d");
            for (int i = 0; i < 30; i++)
            {
                var d = dayStart.AddDays(i);
                dto.DailyChartLabels.Add(d.ToString("dd MMM", culture));
                dto.DailyChartData.Add(daily.TryGetValue(d.ToString("yyyy-MM-dd"), out var c) ? c : 0);
            }

            return dto;
        }

        public async Task<Dictionary<string, int>> GetBookingCountsByTourAsync()
        {
            var list = await _reservationCollection.Aggregate()
                .Match(x => x.Status != ReservationStatus.Cancelled)
                .Group(x => x.TourId, g => new { TourId = g.Key, Count = g.Count() })
                .ToListAsync();

            return list
                .Where(x => !string.IsNullOrEmpty(x.TourId))
                .ToDictionary(x => x.TourId, x => x.Count);
        }

        // Rezervasyonları tarihe göre ($dateToString ile "2026-10" veya "2026-10-16") gruplayıp sayar
        private async Task<Dictionary<string, int>> CountReservationsByDateAsync(DateTime fromUtc, string format)
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("ReservationDate", new BsonDocument("$gte", fromUtc))),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", new BsonDocument("$dateToString", new BsonDocument
                        {
                            { "format", format },
                            { "date", "$ReservationDate" },
                            { "timezone", TimeZone }
                        })
                    },
                    { "count", new BsonDocument("$sum", 1) }
                })
            };

            var result = await _reservationCollection
                .Aggregate<BsonDocument>(PipelineDefinition<Reservation, BsonDocument>.Create(pipeline))
                .ToListAsync();

            return result.ToDictionary(x => x["_id"].AsString, x => x["count"].ToInt32());
        }

        // Tur bazlı: rezervasyon sayısı, kişi sayısı, gelir + ($lookup ile) turun güncel kapak görseli
        private async Task<List<TopSellingTourDto>> GetTourSalesAsync()
        {
            var pipeline = new[]
            {
                // İptal edilenler (Status = 2) hariç
                new BsonDocument("$match", new BsonDocument("Status", new BsonDocument("$ne", (int)ReservationStatus.Cancelled))),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$TourId" },
                    { "TourTitle", new BsonDocument("$first", "$TourTitle") },
                    { "Bookings", new BsonDocument("$sum", 1) },
                    { "Participants", new BsonDocument("$sum", new BsonDocument("$add", new BsonArray { "$AdultCount", "$ChildCount" })) },
                    // $toDecimal: TotalPrice string ya da sayı olarak saklanmış olsa da doğru toplar
                    { "Revenue", new BsonDocument("$sum", new BsonDocument("$toDecimal", "$TotalPrice")) }
                }),
                // TourId (string) -> ObjectId, sonra Tours koleksiyonuna join
                new BsonDocument("$addFields", new BsonDocument("tourOid", ToObjectId("$_id"))),
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", _tourCollectionName },
                    { "localField", "tourOid" },
                    { "foreignField", "_id" },
                    { "as", "tour" }
                }),
                new BsonDocument("$unwind", new BsonDocument { { "path", "$tour" }, { "preserveNullAndEmptyArrays", true } }),
                new BsonDocument("$project", new BsonDocument
                {
                    { "TourTitle", 1 }, { "Bookings", 1 }, { "Participants", 1 }, { "Revenue", 1 },
                    { "CoverImageUrl", "$tour.CoverImageUrl" }
                })
            };

            var result = await _reservationCollection
                .Aggregate<BsonDocument>(PipelineDefinition<Reservation, BsonDocument>.Create(pipeline))
                .ToListAsync();

            return result.Select(x => new TopSellingTourDto
            {
                TourId = x["_id"].IsString ? x["_id"].AsString : x["_id"].ToString()!,
                TourTitle = GetString(x, "TourTitle"),
                TotalBookings = x["Bookings"].ToInt32(),
                TotalParticipants = x["Participants"].ToInt32(),
                TotalRevenue = ToDecimal(x["Revenue"]),
                CoverImageUrl = GetString(x, "CoverImageUrl")
            }).ToList();
        }

        // Case: Destinasyon bazlı rezervasyon sayısı
        // Reservations --($lookup)--> Tours (DestinationId) --($group)--> --($lookup)--> Destinations (şehir/ülke adı)
        private async Task<List<DestinationReservationDto>> GetReservationsByDestinationAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("Status", new BsonDocument("$ne", (int)ReservationStatus.Cancelled))),
                new BsonDocument("$addFields", new BsonDocument("tourOid", ToObjectId("$TourId"))),
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", _tourCollectionName },
                    { "localField", "tourOid" },
                    { "foreignField", "_id" },
                    { "as", "tour" }
                }),
                new BsonDocument("$unwind", "$tour"),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$tour.DestinationId" },
                    { "ReservationCount", new BsonDocument("$sum", 1) },
                    { "Participants", new BsonDocument("$sum", new BsonDocument("$add", new BsonArray { "$AdultCount", "$ChildCount" })) },
                    { "Revenue", new BsonDocument("$sum", new BsonDocument("$toDecimal", "$TotalPrice")) },
                    // Destinasyon kaydı bulunamazsa turun şehir/ülke bilgisini yedek olarak kullan
                    { "FallbackCity", new BsonDocument("$first", "$tour.City") },
                    { "FallbackCountry", new BsonDocument("$first", "$tour.Country") }
                }),
                new BsonDocument("$addFields", new BsonDocument("destOid", ToObjectId("$_id"))),
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", _destinationCollectionName },
                    { "localField", "destOid" },
                    { "foreignField", "_id" },
                    { "as", "dest" }
                }),
                new BsonDocument("$unwind", new BsonDocument { { "path", "$dest" }, { "preserveNullAndEmptyArrays", true } }),
                new BsonDocument("$project", new BsonDocument
                {
                    { "ReservationCount", 1 }, { "Participants", 1 }, { "Revenue", 1 },
                    { "City", new BsonDocument("$ifNull", new BsonArray { "$dest.City", "$FallbackCity" }) },
                    { "Country", new BsonDocument("$ifNull", new BsonArray { "$dest.Country", "$FallbackCountry" }) }
                }),
                new BsonDocument("$sort", new BsonDocument("ReservationCount", -1))
            };

            var result = await _reservationCollection
                .Aggregate<BsonDocument>(PipelineDefinition<Reservation, BsonDocument>.Create(pipeline))
                .ToListAsync();

            return result.Select(x =>
            {
                var city = GetString(x, "City");
                var country = GetString(x, "Country");
                return new DestinationReservationDto
                {
                    DestinationId = x["_id"].IsBsonNull ? string.Empty : x["_id"].ToString()!,
                    DestinationName = string.Join(", ", new[] { city, country }.Where(s => !string.IsNullOrWhiteSpace(s))),
                    ReservationCount = x["ReservationCount"].ToInt32(),
                    TotalParticipants = x["Participants"].ToInt32(),
                    TotalRevenue = ToDecimal(x["Revenue"])
                };
            }).ToList();
        }

        private static DateTime GetLocalNow()
        {
            try { return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, TimeZone); }
            catch (TimeZoneNotFoundException) { return DateTime.Now; }
        }

        // Geçersiz/boş Id'lerde hata vermeden null döndüren $convert
        private static BsonDocument ToObjectId(string field) =>
            new BsonDocument("$convert", new BsonDocument
            {
                { "input", field },
                { "to", "objectId" },
                { "onError", BsonNull.Value },
                { "onNull", BsonNull.Value }
            });

        private static string GetString(BsonDocument doc, string name) =>
            doc.TryGetValue(name, out var v) && v.IsString ? v.AsString : string.Empty;

        private static decimal ToDecimal(BsonValue v) =>
            v.IsDecimal128 ? (decimal)v.AsDecimal128 : v.IsNumeric ? (decimal)v.ToDouble() : 0m;
    }
}