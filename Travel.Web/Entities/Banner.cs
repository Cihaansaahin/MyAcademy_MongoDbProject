using Travel.Web.Entities.Common;

namespace Travel.Web.Entities
{
    public class Banner : BaseEntity
    {
        public required String ImageUrl { get; set; }
        public required String Title { get; set; }
        public required String Description { get; set; }
    }
}
