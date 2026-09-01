using Travel.Web.Entities.Common;

namespace Travel.Web.Entities
{
    public class Banner : BaseEntity
    {
        public String ImageUrl { get; set; }
        public String Title { get; set; }
        public String Description { get; set; }
    }
}
