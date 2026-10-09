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


        // Case Madde 7 & 8: Tarih seçimi zorunlu, kontenjan kontrolü, ücret sunucuda hesaplanır
        public async Task<bool> CreateReservationAsync(CreateReservationDto dto)
        {
            // 1. Temel kontroller
            if (dto.AdultCount < 1 || dto.ChildCount < 0)
                return false;
            if (string.IsNullOrWhiteSpace(dto.TourId) || string.IsNullOrWhiteSpace(dto.TourDateId))
                return false;

            var tour = await _tourService.GetByIdAsync(dto.TourId);
            if (tour == null || !tour.IsActive)
                return false;

            var selectedDate = tour.TourDates?.FirstOrDefault(d => d.Id == dto.TourDateId);
            if (selectedDate == null || !selectedDate.IsActive || selectedDate.StartDate < DateTime.UtcNow.Date)
                return false;

            int totalCount = dto.AdultCount + dto.ChildCount;

            // 2. Kontenjanı düş — yetmiyorsa rezervasyon ALINMAZ
            var capacityOk = await _tourService.DecreaseCapacityAsync(dto.TourId, dto.TourDateId, totalCount);
            if (!capacityOk)
                return false;

            // 3. Rezervasyonu hazırla — fiyat tarayıcıdan değil, veritabanındaki turdan hesaplanır
            var reservation = _mapper.Map<Reservation>(dto);
            reservation.TourTitle = tour.Title?.Tr ?? "Tur";
            reservation.SelectedTourDate = selectedDate.StartDate;
            reservation.TotalPrice = (dto.AdultCount * tour.Price) + (dto.ChildCount * tour.Price * 0.5m);
            reservation.Status = ReservationStatus.Pending;
            reservation.ReservationDate = DateTime.UtcNow;

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