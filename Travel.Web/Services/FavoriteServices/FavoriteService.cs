using MongoDB.Driver;
using Travel.Web.Entities;
using Travel.Web.Settings;

namespace Travel.Web.Services.FavoriteServices
{
   
    public class FavoriteService : IFavoriteService
    {
        private readonly IMongoCollection<Favorite> _favoriteCollection;

        public FavoriteService(IDataBaseSettings databaseSettings)
        {
            var client = new MongoClient(databaseSettings.ConnectionString);
            var database = client.GetDatabase(databaseSettings.DataBaseName);
            _favoriteCollection = database.GetCollection<Favorite>(databaseSettings.FavoriteCollectionName);
        }

        public async Task<bool> ToggleAsync(string userId, string tourId)
        {
            // Önce silmeyi dene: silindiyse zaten favorideydi → artık değil
            var deleted = await _favoriteCollection.DeleteOneAsync(x => x.UserId == userId && x.TourId == tourId);
            if (deleted.DeletedCount > 0)
                return false;

            await _favoriteCollection.InsertOneAsync(new Favorite
            {
                UserId = userId,
                TourId = tourId,
                AddedDate = DateTime.UtcNow
            });
            return true;
        }

        public async Task<bool> IsFavoriteAsync(string userId, string tourId)
        {
            return await _favoriteCollection.Find(x => x.UserId == userId && x.TourId == tourId).AnyAsync();
        }

        public async Task<List<string>> GetTourIdsByUserAsync(string userId)
        {
            return await _favoriteCollection.Find(x => x.UserId == userId)
                .SortByDescending(x => x.AddedDate)
                .Project(x => x.TourId)
                .ToListAsync();
        }
    }
}