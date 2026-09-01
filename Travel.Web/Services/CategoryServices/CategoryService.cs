using AutoMapper;
using MongoDB.Driver;
using Travel.Web.DTOs.CategoryDtos;
using Travel.Web.Entities;
using Travel.Web.Settings;

namespace Travel.Web.Services.CategoryServices
{
    public class CategoryService : ICategoryService
    {
        private readonly IMongoCollection<Category> _categoryCollection;
        private readonly IMapper _mapper;

        public CategoryService(IDataBaseSettings databaseSettings, IMapper mapper)
        {
            var client = new MongoClient(databaseSettings.ConnectionString);
            var database = client.GetDatabase(databaseSettings.DataBaseName);
            _categoryCollection = database.GetCollection<Category>(databaseSettings.CategoryCollectionName);
            _mapper = mapper;
        }

        public async Task<List<ResultCategoryDto>> GetAllAsync()
        {
            var categories = await _categoryCollection.Find(_ => true).ToListAsync();
            return _mapper.Map<List<ResultCategoryDto>>(categories);
        }

        public async Task<ResultCategoryDto> GetByIdAsync(string id)
        {
            var category = await _categoryCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
            return _mapper.Map<ResultCategoryDto>(category);
        }

        public async Task CreateAsync(CreateCategoryDto createCategoryDto)
        {
            var category = _mapper.Map<Category>(createCategoryDto);
            await _categoryCollection.InsertOneAsync(category);
        }

        public async Task UpdateAsync(UpdateCategoryDto updateCategoryDto)
        {
            var category = _mapper.Map<Category>(updateCategoryDto);
            await _categoryCollection.FindOneAndReplaceAsync(x => x.Id == category.Id, category);
        }

        public async Task DeleteAsync(string id)
        {
            await _categoryCollection.DeleteOneAsync(x => x.Id == id);
        }
    }
}