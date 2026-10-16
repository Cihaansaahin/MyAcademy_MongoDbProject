using Travel.Web.DTOs.DashboardDtos;

namespace Travel.Web.Services.DashboardServices
{
    public interface IDashboardService
    {
        Task<DashboardStatisticsDto> GetDashboardStatisticsAsync();

        // Ana sayfa "En Çok Tercih Edilen Turlar" için: TourId -> rezervasyon sayısı (iptaller hariç, Aggregation)
        Task<Dictionary<string, int>> GetBookingCountsByTourAsync();
    }
}