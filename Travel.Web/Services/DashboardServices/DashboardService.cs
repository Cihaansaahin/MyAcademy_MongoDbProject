using MongoDB.Bson;
using MongoDB.Driver;
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

        public DashboardService(IDatabaseSettings databaseSettings)
        {
            var client = new MongoClient(databaseSettings.ConnectionString);
            var database = client.GetDatabase(databaseSettings.DatabaseName);

            _tourCollection = database.GetCollection<Tour>(databaseSettings.TourCollectionName);
            _reservationCollection = database.GetCollection<Reservation>(databaseSettings.ReservationCollectionName);
            _questionCollection = database.GetCollection<Question>(databaseSettings.QuestionCollectionName);
            _categoryCollection = database.GetCollection<Category>(databaseSettings.CategoryCollectionName);
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

            dto.PendingQuestionsCount = await _questionCollection.CountDocumentsAsync(x => string.IsNullOrEmpty(x.AnswerText));

            // 2. Aggregation Pipeline: En Çok Satan 5 Tur & Gelirler (Case Madde 16)
            var top5Pipeline = _reservationCollection.Aggregate()
                .Match(x => x.Status != ReservationStatus.Cancelled)
                .Group(x => new { x.TourId, x.TourTitle }, g => new TopSellingTourDto
                {
                    TourId = g.Key.TourId,
                    TourTitle = g.Key.TourTitle,
                    TotalBookings = g.Sum(x => x.TotalParticipants),
                    TotalRevenue = g.Sum(x => x.TotalPrice)
                })
                .SortByDescending(x => x.TotalBookings)
                .Limit(5);

            dto.Top5SellingTours = await top5Pipeline.ToListAsync();

            if (dto.Top5SellingTours.Any())
            {
                dto.TopBookedTourName = dto.Top5SellingTours.First().TourTitle;
            }

            // 3. Aggregation Pipeline: Kategori Bazında Tur Sayısı (Case Madde 16)
            var categoryGroupPipeline = _tourCollection.Aggregate()
                .Group(x => x.CategoryId, g => new
                {
                    CategoryId = g.Key,
                    Count = g.Count()
                });

            var rawCategoryCounts = await categoryGroupPipeline.ToListAsync();
            var categories = await _categoryCollection.Find(_ => true).ToListAsync();

            foreach (var item in rawCategoryCounts)
            {
                var category = categories.FirstOrDefault(c => c.Id == item.CategoryId);
                dto.ToursByCategory.Add(new CategoryTourCountDto
                {
                    CategoryId = item.CategoryId,
                    CategoryName = category != null ? category.Name.Value : "Belirtilmemiş",
                    TourCount = item.Count
                });
            }

            // 4. Son 6 Aylık Rezervasyon Verisi (Chart.js için)
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-5);
            var firstDayOfSixMonthsAgo = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var reservations = await _reservationCollection.Find(x => x.ReservationDate >= firstDayOfSixMonthsAgo).ToListAsync();

            for (int i = 5; i >= 0; i--)
            {
                var targetMonth = DateTime.UtcNow.AddMonths(-i);
                var monthLabel = targetMonth.ToString("MMMM yyyy");
                var count = reservations.Count(r => r.ReservationDate.Year == targetMonth.Year && r.ReservationDate.Month == targetMonth.Month);

                dto.MonthlyChartLabels.Add(monthLabel);
                dto.MonthlyChartData.Add(count);
            }

            return dto;
        }
    }
}