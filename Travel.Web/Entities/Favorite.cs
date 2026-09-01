using Travel.Web.Entities.Common;

namespace Travel.Web.Entities
{
    public class Favorite : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string TourId { get; set; } = string.Empty;
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
    }
}
