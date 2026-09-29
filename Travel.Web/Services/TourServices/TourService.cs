using AutoMapper;
using MongoDB.Driver;
using Travel.Web.DTOs.TourDtos;
using Travel.Web.Entities;
using Travel.Web.Settings;

namespace Travel.Web.Services.TourServices
{
    public class TourService : ITourService
    {
        private readonly IMongoCollection<Tour> _tourCollection;
        private readonly IMapper _mapper;

        public TourService(IDataBaseSettings databaseSettings, IMapper mapper)
        {
            var client = new MongoClient(databaseSettings.ConnectionString);
            var database = client.GetDatabase(databaseSettings.DataBaseName);
            _tourCollection = database.GetCollection<Tour>(databaseSettings.TourCollectionName);
            _mapper = mapper;
        }

        public async Task<List<ResultTourDto>> GetAllAsync()
        {
            var tours = await _tourCollection.Find(_ => true).ToListAsync();
            return _mapper.Map<List<ResultTourDto>>(tours);
        }

        public async Task<ResultTourDto> GetByIdAsync(string id)
        {
            var tour = await _tourCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
            return _mapper.Map<ResultTourDto>(tour);
        }

        public async Task CreateAsync(CreateTourDto createTourDto)
        {
            var tour = _mapper.Map<Tour>(createTourDto);

            // Yeni eklenen tarihlerin ID ve RemainingCapacity değerlerini garantiye al
            if (tour.TourDates != null)
            {
                foreach (var td in tour.TourDates)
                {
                    if (string.IsNullOrEmpty(td.Id))
                        td.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();

                    if (td.RemainingCapacity <= 0)
                        td.RemainingCapacity = td.Capacity > 0 ? td.Capacity : 28;
                }
            }

            await _tourCollection.InsertOneAsync(tour);
        }

        public async Task UpdateAsync(UpdateTourDto updateTourDto)
        {
            var tour = _mapper.Map<Tour>(updateTourDto);

            if (tour.TourDates != null)
            {
                foreach (var td in tour.TourDates)
                {
                    if (string.IsNullOrEmpty(td.Id))
                        td.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();

                    if (td.RemainingCapacity <= 0)
                        td.RemainingCapacity = td.Capacity > 0 ? td.Capacity : 28;
                }
            }

            await _tourCollection.FindOneAndReplaceAsync(x => x.Id == tour.Id, tour);
        }

        public async Task DeleteAsync(string id)
        {
            await _tourCollection.DeleteOneAsync(x => x.Id == id);
        }

        // Kontenjanı garantili olarak düşürme
        public async Task<bool> DecreaseCapacityAsync(string tourId, string tourDateId, int count)
        {
            if (string.IsNullOrWhiteSpace(tourId))
                return false;

            var tour = await _tourCollection.Find(x => x.Id == tourId).FirstOrDefaultAsync();
            if (tour == null || tour.TourDates == null || !tour.TourDates.Any())
                return false;

            // Hedef tarihi bul: ID eşleşmesi ara, yoksa en yakın geçerli tarihi seç
            var targetDate = (!string.IsNullOrEmpty(tourDateId) ? tour.TourDates.FirstOrDefault(d => d.Id == tourDateId) : null)
                             ?? tour.TourDates.FirstOrDefault(d => d.StartDate >= DateTime.UtcNow)
                             ?? tour.TourDates.FirstOrDefault();

            if (targetDate == null)
                return false;

            // Kalan kontenjan sıfır veya eksik kalmışsa kapasiteye eşitle
            if (targetDate.RemainingCapacity <= 0)
            {
                targetDate.RemainingCapacity = targetDate.Capacity > 0 ? targetDate.Capacity : 28;
            }

            if (targetDate.RemainingCapacity < count)
                return false;

            targetDate.RemainingCapacity -= count;

            var result = await _tourCollection.ReplaceOneAsync(x => x.Id == tourId, tour);
            return result.ModifiedCount > 0 || result.MatchedCount > 0;
        }

        // İptal durumunda kontenjan iadesi
        public async Task<bool> IncreaseCapacityAsync(string tourId, string tourDateId, int count)
        {
            if (string.IsNullOrWhiteSpace(tourId))
                return false;

            var tour = await _tourCollection.Find(x => x.Id == tourId).FirstOrDefaultAsync();
            if (tour == null || tour.TourDates == null || !tour.TourDates.Any())
                return false;

            var targetDate = (!string.IsNullOrEmpty(tourDateId) ? tour.TourDates.FirstOrDefault(d => d.Id == tourDateId) : null)
                             ?? tour.TourDates.FirstOrDefault();

            if (targetDate == null)
                return false;

            targetDate.RemainingCapacity = Math.Min(targetDate.Capacity, targetDate.RemainingCapacity + count);

            var result = await _tourCollection.ReplaceOneAsync(x => x.Id == tourId, tour);
            return result.ModifiedCount > 0 || result.MatchedCount > 0;
        }

        public async Task<List<ResultTourDto>> GetPopularToursAsync(int count = 6)
        {
            var tours = await _tourCollection.Find(x => x.IsActive && x.IsPopular).Limit(count).ToListAsync();
            return _mapper.Map<List<ResultTourDto>>(tours);
        }
    }
}