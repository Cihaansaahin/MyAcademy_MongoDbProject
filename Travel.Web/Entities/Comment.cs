using Travel.Web.Entities.Common;

namespace Travel.Web.Entities
{
    public class Comment : BaseEntity
    {
        public string TourId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; } // 1 - 5
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsApproved { get; set; } = true;
    }
}
