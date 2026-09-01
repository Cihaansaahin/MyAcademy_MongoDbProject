using Travel.Web.DTOs.DestinationDtos;

namespace Travel.Web.Services.DestinationServices
{
    public interface IDestinationService
    {
        Task<List<ResultDestinationDto>> GetAllAsync();
        Task<ResultDestinationDto> GetByIdAsync(string id);
        Task CreateAsync(CreateDestinationDto createDestinationDto);
        Task UpdateAsync(UpdateDestinationDto updateDestinationDto);
        Task DeleteAsync(string id);
        Task<List<ResultDestinationDto>> GetPopularDestinationsAsync(int count = 6);
    }
}