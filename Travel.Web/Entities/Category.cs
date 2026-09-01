using Travel.Web.Entities.Common;

namespace Travel.Web.Entities
{
    public class Category : BaseEntity
    {
        public LocalizedString Name { get; set; } = new();
        public string IconUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
