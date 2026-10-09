using AutoMapper;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
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

                    td.RemainingCapacity = td.Capacity; // Yeni tarih: kalan = toplam kontenjan
                }
            }

            await _tourCollection.InsertOneAsync(tour);
        }

        public async Task UpdateAsync(UpdateTourDto updateTourDto)
        {
            var tour = _mapper.Map<Tour>(updateTourDto);
            var existing = await _tourCollection.Find(x => x.Id == tour.Id).FirstOrDefaultAsync();

            foreach (var td in tour.TourDates ?? new List<TourDateItem>())
            {
                var old = existing?.TourDates?.FirstOrDefault(d => d.Id == td.Id);

                if (old == null)
                {
                    // Yeni eklenen tarih
                    if (string.IsNullOrEmpty(td.Id))
                        td.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
                    td.RemainingCapacity = td.Capacity;
                }
                else
                {
                    // Var olan tarih: satılmış koltukları koru.
                    // Admin kontenjanı 20'den 30'a çıkarırsa kalan da 10 artar.
                    var sold = old.Capacity - old.RemainingCapacity;
                    td.RemainingCapacity = Math.Max(0, td.Capacity - sold);
                }
            }

            await _tourCollection.ReplaceOneAsync(x => x.Id == tour.Id, tour);
        }

        public async Task DeleteAsync(string id)
        {
            await _tourCollection.DeleteOneAsync(x => x.Id == id);
        }

        // Case Madde 7: Kontenjanı ATOMİK olarak düşür.
        // Filtre "bu tarihte en az count kadar yer var mı?" diye bakar; yoksa hiçbir şey güncellenmez.
        public async Task<bool> DecreaseCapacityAsync(string tourId, string tourDateId, int count)
        {
            if (string.IsNullOrWhiteSpace(tourId) || string.IsNullOrWhiteSpace(tourDateId) || count <= 0)
                return false;

            var filter = Builders<Tour>.Filter.And(
                Builders<Tour>.Filter.Eq(t => t.Id, tourId),
                Builders<Tour>.Filter.ElemMatch(t => t.TourDates,
                    d => d.Id == tourDateId && d.IsActive && d.RemainingCapacity >= count));

            // FirstMatchingElement() => MongoDB'deki "TourDates.$" (filtrede eşleşen eleman)
            var update = Builders<Tour>.Update
                .Inc(t => t.TourDates.FirstMatchingElement().RemainingCapacity, -count);

            var result = await _tourCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        // Case Madde 8/14: İptalde kontenjanı geri ver (Capacity'yi aşmayacak şekilde)
        public async Task<bool> IncreaseCapacityAsync(string tourId, string tourDateId, int count)
        {
            if (string.IsNullOrWhiteSpace(tourId) || string.IsNullOrWhiteSpace(tourDateId) || count <= 0)
                return false;

            var tour = await _tourCollection.Find(x => x.Id == tourId).FirstOrDefaultAsync();
            var date = tour?.TourDates?.FirstOrDefault(d => d.Id == tourDateId);
            if (date == null)
                return false;

            var newRemaining = Math.Min(date.Capacity, date.RemainingCapacity + count);

            var filter = Builders<Tour>.Filter.And(
                Builders<Tour>.Filter.Eq(t => t.Id, tourId),
                Builders<Tour>.Filter.ElemMatch(t => t.TourDates, d => d.Id == tourDateId));

            var update = Builders<Tour>.Update
                .Set(t => t.TourDates.FirstMatchingElement().RemainingCapacity, newRemaining);

            var result = await _tourCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<List<ResultTourDto>> GetPopularToursAsync(int count = 6)
        {
            var tours = await _tourCollection.Find(x => x.IsActive && x.IsPopular).Limit(count).ToListAsync();
            return _mapper.Map<List<ResultTourDto>>(tours);
        }
    }
}