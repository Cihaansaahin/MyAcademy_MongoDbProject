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
        public string TopBookedTourName { get; set; } = string.Empty;

        // 2. Öne çıkan değerler (Aggregation)
        public decimal TotalRevenue { get; set; }                 // İptal edilmeyen rezervasyonların toplam tutarı
        public TopSellingTourDto? TopRevenueTour { get; set; }    // En çok gelir getiren tur
        public double AverageRating { get; set; }                 // Tüm onaylı yorumların ortalama puanı
        public int TotalReviews { get; set; }

        // 3. Chart.js grafikleri (Aggregation ile tarih bazlı gruplama)
        public List<string> MonthlyChartLabels { get; set; } = new();   // Son 6 ay
        public List<int> MonthlyChartData { get; set; } = new();
        public List<string> DailyChartLabels { get; set; } = new();     // Son 30 gün
        public List<int> DailyChartData { get; set; } = new();

        // 4. Aggregation Pipeline Raporları (Case Madde 16)
        public List<CategoryTourCountDto> ToursByCategory { get; set; } = new();
        public List<TopSellingTourDto> Top5SellingTours { get; set; } = new();
        public List<DestinationReservationDto> ReservationsByDestination { get; set; } = new();
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
        public int TotalBookings { get; set; }       // Rezervasyon sayısı
        public int TotalParticipants { get; set; }   // Toplam kişi
        public decimal TotalRevenue { get; set; }
        public string CoverImageUrl { get; set; } = string.Empty;
    }

    public class DestinationReservationDto
    {
        public string DestinationId { get; set; } = string.Empty;
        public string DestinationName { get; set; } = string.Empty;
        public int ReservationCount { get; set; }
        public int TotalParticipants { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}