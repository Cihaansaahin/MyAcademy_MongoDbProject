using Travel.Web.DTOs.CategoryDtos;

namespace Travel.Web.Services.CategoryServices
{
    public interface ICategoryService
    {
        Task<List<ResultCategoryDto>> GetAllAsync();
        Task<ResultCategoryDto> GetByIdAsync(string id);
        Task CreateAsync(CreateCategoryDto createCategoryDto);
        Task UpdateAsync(UpdateCategoryDto updateCategoryDto);
        Task DeleteAsync(string id);
    }
}