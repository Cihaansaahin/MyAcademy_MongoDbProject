using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Travel.Web.Entities.Common;

namespace Travel.Web.Entities
{
    // 1. Tur Tarihleri ve Kontenjan
    public class TourDateItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Capacity { get; set; }
        public int RemainingCapacity { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // 2. Gün Gün Tur Programı (Itinerary)
    public class TourPlanDay
    {
        public int DayNumber { get; set; }
        public LocalizedString Title { get; set; } = new();
        public LocalizedString Description { get; set; } = new();
        public string City { get; set; } = string.Empty;
        public string? MealInfo { get; set; } // Opsiyonel
        public string? TransportInfo { get; set; } // Opsiyonel
    }
}