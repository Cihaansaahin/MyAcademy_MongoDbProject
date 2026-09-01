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
            await _tourCollection.InsertOneAsync(tour);
        }

        public async Task UpdateAsync(UpdateTourDto updateTourDto)
        {
            var tour = _mapper.Map<Tour>(updateTourDto);
            await _tourCollection.FindOneAndReplaceAsync(x => x.Id == tour.Id, tour);
        }

        public async Task DeleteAsync(string id)
        {
            await _tourCollection.DeleteOneAsync(x => x.Id == id);
        }

        // Case Madde 7: Array içindeki ilgili tarihin kontenjanını atomik düşürme
        public async Task<bool> DecreaseCapacityAsync(string tourId, string tourDateId, int count)
        {
            var filter = Builders<Tour>.Filter.And(
                Builders<Tour>.Filter.Eq(x => x.Id, tourId),
                Builders<Tour>.Filter.ElemMatch(x => x.TourDates, d => d.Id == tourDateId && d.RemainingCapacity >= count)
            );

            var update = Builders<Tour>.Update.Inc("TourDates.$.RemainingCapacity", -count);
            var result = await _tourCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        // İptal durumunda kontenjan iadesi
        public async Task<bool> IncreaseCapacityAsync(string tourId, string tourDateId, int count)
        {
            var filter = Builders<Tour>.Filter.And(
                Builders<Tour>.Filter.Eq(x => x.Id, tourId),
                Builders<Tour>.Filter.ElemMatch(x => x.TourDates, d => d.Id == tourDateId)
            );

            var update = Builders<Tour>.Update.Inc("TourDates.$.RemainingCapacity", count);
            var result = await _tourCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        // Case Madde 4: Popüler turları listeleme
        public async Task<List<ResultTourDto>> GetPopularToursAsync(int count = 6)
        {
            var tours = await _tourCollection.Find(x => x.IsActive && x.IsPopular).Limit(count).ToListAsync();
            return _mapper.Map<List<ResultTourDto>>(tours);
        }
    }
}