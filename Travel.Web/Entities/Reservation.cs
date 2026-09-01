using Travel.Web.Entities.Common;

namespace Travel.Web.Entities
{
    public enum ReservationStatus
    {
        Pending = 0,   // Bekliyor
        Approved = 1,  // Onaylandı
        Cancelled = 2  // İptal Edildi
    }

    public class Reservation : BaseEntity
    {
        public string TourId { get; set; } = string.Empty;
        public string TourTitle { get; set; } = string.Empty;
        public string TourDateId { get; set; } = string.Empty; // Seçilen tarihin Id'si
        public DateTime SelectedTourDate { get; set; }

        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public int AdultCount { get; set; }
        public int ChildCount { get; set; }
        public int TotalParticipants => AdultCount + ChildCount;

        public decimal TotalPrice { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
        public DateTime ReservationDate { get; set; } = DateTime.UtcNow;
    }
}