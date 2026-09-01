namespace Travel.Web.DTOs.ReservationDtos
{
    public class CreateReservationDto
    {
        public string TourId { get; set; } = string.Empty;
        public string TourTitle { get; set; } = string.Empty;
        public string TourDateId { get; set; } = string.Empty;
        public DateTime SelectedTourDate { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int AdultCount { get; set; }
        public int ChildCount { get; set; }
        public decimal UnitPrice { get; set; }
        // Case kuralı: Toplam ücret otomatik hesaplanır[cite: 1]
        public decimal TotalPrice => (AdultCount * UnitPrice) + (ChildCount * (UnitPrice / 2));
    }
}