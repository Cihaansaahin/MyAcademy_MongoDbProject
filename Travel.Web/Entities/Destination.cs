using Travel.Web.Entities.Common;

namespace Travel.Web.Entities
{
    public class Destination : BaseEntity
    {
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPopular { get; set; } = false;
    }
}
