namespace Travel.Web.DTOs.DashboardDtos
{
    public class DashboardStatisticsDto
    {
        // 1. Özet Sayaçlar (Case Madde 13)
        public long TotalTours { get; set; }
        public long ActiveTours { get; set; }
        public long PassiveTours { get; set; }
        public long TotalReservations { get; set; }
        public long ThisMonthReservationsCount { get; set; }
        public long PendingQuestionsCount { get; set; }
        public string TopBookedTourName { get; set; } = "Henüz rezervasyon yok";

        // 2. Chart.js İçin Son 6 Aylık Rezervasyon Grafiği
        public List<string> MonthlyChartLabels { get; set; } = new();
        public List<int> MonthlyChartData { get; set; } = new();

        // 3. Aggregation Pipeline Raporları (Case Madde 16)
        public List<CategoryTourCountDto> ToursByCategory { get; set; } = new();
        public List<TopSellingTourDto> Top5SellingTours { get; set; } = new();
    }

    public class CategoryTourCountDto
    {
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int TourCount { get; set; }
    }

    public class TopSellingTourDto
    {
        public string TourId { get; set; } = string.Empty;
        public string TourTitle { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}