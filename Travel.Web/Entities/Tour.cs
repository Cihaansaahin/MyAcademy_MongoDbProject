using Travel.Web.Entities.Common;

namespace Travel.Web.Entities
{
    public class Tour : BaseEntity
    {
        public LocalizedString Title { get; set; } = new();
        public LocalizedString Description { get; set; } = new();
        public decimal Price { get; set; }
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string DestinationId { get; set; } = string.Empty;
        public int DurationDays { get; set; } // Süre (Gün)
        public string CoverImageUrl { get; set; } = string.Empty;
        public List<string> GalleryImageUrls { get; set; } = new();
        public List<string> Features { get; set; } = new(); // Öne çıkan özellikler (Örn: "Ücretsiz İptal", "Rehber Dahil")
        public bool IsActive { get; set; } = true;
        public bool IsPopular { get; set; } = false;

        // Nested Listeler
        public List<TourDateItem> TourDates { get; set; } = new();
        public List<TourPlanDay> Itinerary { get; set; } = new();

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}