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

        // Case Madde 7 & 8: Kontenjanı kontrol edip düşürme ve rezervasyonu kaydetme
        public async Task<bool> CreateReservationAsync(CreateReservationDto dto)
        {
            int totalCount = dto.AdultCount + dto.ChildCount;

            // 1. Kontenjanı atomik olarak düşürmeyi dene
            var capacityAvailable = await _tourService.DecreaseCapacityAsync(dto.TourId, dto.TourDateId, totalCount);
            if (!capacityAvailable)
                return false; // Kontenjan yetersiz

            // 2. Kontenjan düştüyse rezervasyonu kaydet
            var reservation = _mapper.Map<Reservation>(dto);
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

            // Kontenjanı iade et
            await _tourService.IncreaseCapacityAsync(reservation.TourId, reservation.TourDateId, reservation.TotalParticipants);
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
    }
}