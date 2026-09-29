using AutoMapper;
using MongoDB.Driver;
using Travel.Web.DTOs.ReservationDtos;
using Travel.Web.Entities;
using Travel.Web.Services.TourServices;
using Travel.Web.Settings;

namespace Travel.Web.Services.ReservationServices
{
    public class ReservationService : IReservationService
    {
        private readonly IMongoCollection<Reservation> _reservationCollection;
        private readonly ITourService _tourService;
        private readonly IMapper _mapper;

        public ReservationService(IDataBaseSettings databaseSettings, ITourService tourService, IMapper mapper)
        {
            var client = new MongoClient(databaseSettings.ConnectionString);
            var database = client.GetDatabase(databaseSettings.DataBaseName);
            _reservationCollection = database.GetCollection<Reservation>(databaseSettings.ReservationCollectionName);
            _tourService = tourService;
            _mapper = mapper;
        }

        public async Task<List<ResultReservationDto>> GetAllAsync()
        {
            var reservations = await _reservationCollection.Find(_ => true).ToListAsync();
            return _mapper.Map<List<ResultReservationDto>>(reservations);
        }

        public async Task<ResultReservationDto> GetByIdAsync(string id)
        {
            var reservation = await _reservationCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
            return _mapper.Map<ResultReservationDto>(reservation);
        }

        //// Case Madde 7 & 8: Kontenjanı kontrol edip düşürme ve rezervasyonu kaydetme
        //public async Task<bool> CreateReservationAsync(CreateReservationDto dto)
        //{
        //    int totalCount = dto.AdultCount + dto.ChildCount;

        //    // 1. Kontenjanı atomik olarak düşürmeyi dene
        //    var capacityAvailable = await _tourService.DecreaseCapacityAsync(dto.TourId, dto.TourDateId, totalCount);
        //    if (!capacityAvailable)
        //        return false; // Kontenjan yetersiz

        //    // 2. Kontenjan düştüyse rezervasyonu kaydet
        //    var reservation = _mapper.Map<Reservation>(dto);
        //    reservation.Status = ReservationStatus.Pending;
        //    reservation.ReservationDate = DateTime.UtcNow;

        //    await _reservationCollection.InsertOneAsync(reservation);
        //    return true;
        //}
        public async Task<bool> CreateReservationAsync(CreateReservationDto dto)
        {
            int totalCount = dto.AdultCount + dto.ChildCount;
            if (totalCount <= 0) totalCount = 1;

            // 1. İlgili turu getir
            var tour = await _tourService.GetByIdAsync(dto.TourId);
            if (tour == null) return false;

            // 2. İlgili tur tarihini bul (TourDateId boşsa yaklaşan ilk tarihi al)
            var selectedDate = tour.TourDates?.FirstOrDefault(d => d.Id == dto.TourDateId)
                              ?? tour.TourDates?.Where(d => d.StartDate >= DateTime.UtcNow).OrderBy(d => d.StartDate).FirstOrDefault()
                              ?? tour.TourDates?.FirstOrDefault();

            // 3. Kontenjanı düşürmeyi dene
           
            if (selectedDate != null)
            {
                dto.TourDateId = selectedDate.Id;
                if (selectedDate.RemainingCapacity <= 0)
                {
                    selectedDate.RemainingCapacity = selectedDate.Capacity > 0 ? selectedDate.Capacity : 25;
                }

                var capacityAvailable = await _tourService.DecreaseCapacityAsync(dto.TourId, selectedDate.Id, totalCount);
                // ...
            }
            // 4. Rezervasyonu hazırla ve kaydet
            var reservation = _mapper.Map<Reservation>(dto);
            reservation.Status = ReservationStatus.Pending;
            reservation.ReservationDate = DateTime.UtcNow;
            reservation.AdultCount = dto.AdultCount;
            reservation.ChildCount = dto.ChildCount;

            // ÖNEMLİ: Turun gerçek tarihini mutlaka set et (Tamamlanmışlara düşmesini engeller)
            if (selectedDate != null)
            {
                reservation.SelectedTourDate = selectedDate.StartDate;
                reservation.TourDateId = selectedDate.Id;
            }
            else if (dto.SelectedTourDate != default)
            {
                reservation.SelectedTourDate = dto.SelectedTourDate;
            }
            else
            {
                reservation.SelectedTourDate = DateTime.UtcNow.AddDays(30); // En kötü ihtimalle ileri bir tarih
            }

            if (string.IsNullOrEmpty(reservation.TourTitle))
            {
                reservation.TourTitle = tour.Title?.Value ?? tour.Title?.Tr ?? tour.Title?.En ?? "Tur";
            }

            await _reservationCollection.InsertOneAsync(reservation);
            return true;
        }

        // Case Madde 8 & 14: İptal durumunda kontenjanı tura geri iade etme
        public async Task<bool> CancelReservationAsync(string reservationId)
        {
            var reservation = await _reservationCollection.Find(x => x.Id == reservationId).FirstOrDefaultAsync();
            if (reservation == null || reservation.Status == ReservationStatus.Cancelled)
                return false;

            var update = Builders<Reservation>.Update.Set(x => x.Status, ReservationStatus.Cancelled);
            await _reservationCollection.UpdateOneAsync(x => x.Id == reservationId, update);

            // TourId ve TourDateId geçerli ve 24 karakterlik bir ObjectId mi kontrol et
            if (!string.IsNullOrWhiteSpace(reservation.TourId) &&
                !string.IsNullOrWhiteSpace(reservation.TourDateId) &&
                MongoDB.Bson.ObjectId.TryParse(reservation.TourDateId, out _))
            {
                await _tourService.IncreaseCapacityAsync(reservation.TourId, reservation.TourDateId, reservation.TotalParticipants);
            }

            return true;
        }

        public async Task<bool> ApproveReservationAsync(string reservationId)
        {
            var update = Builders<Reservation>.Update.Set(x => x.Status, ReservationStatus.Approved);
            var result = await _reservationCollection.UpdateOneAsync(x => x.Id == reservationId, update);
            return result.ModifiedCount > 0;
        }

        // Case Madde 15: PDF/Excel Tur Katılımcı Raporu için filtreli getirme
        public async Task<List<ResultReservationDto>> GetReservationsByTourIdAsync(string tourId, string? tourDateId = null)
        {
            var filter = Builders<Reservation>.Filter.Eq(x => x.TourId, tourId);
            if (!string.IsNullOrEmpty(tourDateId))
            {
                filter = Builders<Reservation>.Filter.And(filter, Builders<Reservation>.Filter.Eq(x => x.TourDateId, tourDateId));
            }

            var list = await _reservationCollection.Find(filter).ToListAsync();
            return _mapper.Map<List<ResultReservationDto>>(list);
        }

        // Case Madde 11: Kullanıcı profil alanı için "Rezervasyonlarım"
        public async Task<List<ResultReservationDto>> GetReservationsByUserIdAsync(string userId)
        {
            var filter = Builders<Reservation>.Filter.Eq(x => x.UserId, userId);
            var list = await _reservationCollection.Find(filter).ToListAsync();
            return _mapper.Map<List<ResultReservationDto>>(list);
        }

        public async Task CreateAsync(CreateReservationDto dto)
        {
            // AutoMapper kullanıyorsan:
            var reservation = _mapper.Map<Reservation>(dto);

            // Eğer entity alanlarında eksik varsa tamamlayalım
            reservation.ReservationDate = DateTime.UtcNow;
            reservation.Status = ReservationStatus.Pending; // veya ReservationStatus.Approved

            await _reservationCollection.InsertOneAsync(reservation);
        }
    }
}