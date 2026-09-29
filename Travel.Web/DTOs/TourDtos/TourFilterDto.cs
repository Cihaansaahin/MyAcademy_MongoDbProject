namespace Travel.Web.DTOs.TourDtos
{
    public class TourFilterDto
    {
        public string? SearchText { get; set; }
        public string? CategoryId { get; set; }
        public string? DestinationId { get; set; }
        public bool? IsActive { get; set; }
    }
}
