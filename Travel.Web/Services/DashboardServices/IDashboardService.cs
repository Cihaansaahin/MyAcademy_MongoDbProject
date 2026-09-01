using Travel.Web.DTOs.DashboardDtos;

namespace Travel.Web.Services.DashboardServices
{
    public interface IDashboardService
    {
        Task<DashboardStatisticsDto> GetDashboardStatisticsAsync();
    }
}