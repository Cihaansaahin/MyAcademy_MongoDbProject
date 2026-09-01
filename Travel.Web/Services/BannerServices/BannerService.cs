using AutoMapper;
using MongoDB.Driver;
using MongoDB.Driver.Linq; 
using System.Reflection;
using Travel.Web.DTOs.BannerDtos;
using Travel.Web.Entities;
using Travel.Web.Settings;

namespace Travel.Web.Services.BannerServices
{
    public class BannerService : IBannerService
    {
        private readonly IMongoCollection<Banner>? _bannerCollection;
        private readonly IMapper _mapper;
        public BannerService(IDataBaseSettings databaseSettings, IMapper mapper)
        {
            var client = new MongoClient(databaseSettings.ConnectionString);
            var database = client.GetDatabase(databaseSettings.DataBaseName);
            _bannerCollection = database.GetCollection<Banner>
                (databaseSettings.BannerCollectionName);
            _mapper = mapper;
        }

        public async Task CreateAsync(CreateBannerDto createBannerDto)
        {
            var banner = _mapper.Map<Banner>(createBannerDto);
            await _bannerCollection.InsertOneAsync(banner);
        }

        public async Task DeleteAsync(string id)
        {
            // delete by string Id (MongoDB ObjectId representation)
            await _bannerCollection.DeleteOneAsync(x => x.Id == id);
        }

       

        public async Task<List<ResultBannerDto>> GetAllAsync()
        {
            var banners = await _bannerCollection.AsQueryable()
                                                 .ToListAsync();
            return _mapper.Map<List<ResultBannerDto>>(banners);
        }

        public async Task<ResultBannerDto> GetByIdAsync(string id)
        {
            // use equality (==) not assignment (=) and async variant
            var banner = await _bannerCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
            return _mapper.Map<ResultBannerDto>(banner);
        }

        public async Task UpdateAsync(UpdateBannerDto updateBannerDto)
        {
            var banner = _mapper.Map<Banner>(updateBannerDto);
             await _bannerCollection.FindOneAndReplaceAsync(x => x.Id == banner.Id,banner);
        }
    }
}

