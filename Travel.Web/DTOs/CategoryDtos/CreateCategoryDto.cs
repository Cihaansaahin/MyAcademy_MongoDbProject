using Travel.Web.Entities.Common;

namespace Travel.Web.DTOs.CategoryDtos
{
    public class CreateCategoryDto
    {
        public LocalizedString Name { get; set; } = new();
        public string IconUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}