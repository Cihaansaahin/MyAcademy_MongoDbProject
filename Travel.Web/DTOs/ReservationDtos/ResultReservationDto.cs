using Travel.Web.Entities;

namespace Travel.Web.DTOs.ReservationDtos
{
    public class ResultReservationDto
    {
        public string Id { get; set; } = string.Empty;
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
        public int TotalParticipants => AdultCount + ChildCount;
        public decimal TotalPrice { get; set; }
        public ReservationStatus Status { get; set; }
        public DateTime ReservationDate { get; set; }
    }
}