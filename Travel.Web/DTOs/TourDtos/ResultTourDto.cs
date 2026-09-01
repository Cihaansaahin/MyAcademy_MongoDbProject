using Travel.Web.Entities;
using Travel.Web.Entities.Common;

namespace Travel.Web.DTOs.TourDtos
{
    public class ResultTourDto
    {
        public string Id { get; set; } = string.Empty;
        public LocalizedString Title { get; set; } = new();
        public LocalizedString Description { get; set; } = new();
        public decimal Price { get; set; }
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string DestinationId { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public string CoverImageUrl { get; set; } = string.Empty;
        public List<string> GalleryImageUrls { get; set; } = new();
        public List<string> Features { get; set; } = new();
        public bool IsActive { get; set; }
        public bool IsPopular { get; set; }

        public List<TourDateItem> TourDates { get; set; } = new();
        public List<TourPlanDay> Itinerary { get; set; } = new();
    }
}