using Travel.Web.DTOs.TourDtos;

namespace Travel.Web.Services.TourServices
{
    public interface ITourService
    {
        Task<List<ResultTourDto>> GetAllAsync();
        Task<ResultTourDto> GetByIdAsync(string id);
        Task CreateAsync(CreateTourDto createTourDto);
        Task UpdateAsync(UpdateTourDto updateTourDto);
        Task DeleteAsync(string id);

        // Case Madde 7: Kontenjan yönetimi ve popüler turlar
        Task<bool> DecreaseCapacityAsync(string tourId, string tourDateId, int count);
        Task<bool> IncreaseCapacityAsync(string tourId, string tourDateId, int count);
        Task<List<ResultTourDto>> GetPopularToursAsync(int count = 6);
   
    }
}