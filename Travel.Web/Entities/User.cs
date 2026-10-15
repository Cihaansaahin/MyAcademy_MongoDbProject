using Travel.Web.Entities.Common;

namespace Travel.Web.Entities
{
    
    public class User : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;         
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;   
        public string Role { get; set; } = "Member";               // "Member" veya "Admin"
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

       
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
