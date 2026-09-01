using Travel.Web.DTOs.ReservationDtos;

namespace Travel.Web.Services.ReservationServices
{
    public interface IReservationService
    {
        Task<List<ResultReservationDto>> GetAllAsync();
        Task<ResultReservationDto> GetByIdAsync(string id);
        Task<bool> CreateReservationAsync(CreateReservationDto createReservationDto);
        Task<bool> CancelReservationAsync(string reservationId);
        Task<bool> ApproveReservationAsync(string reservationId);
        Task<List<ResultReservationDto>> GetReservationsByTourIdAsync(string tourId, string? tourDateId = null);
        Task<List<ResultReservationDto>> GetReservationsByUserIdAsync(string userId);
    }
}