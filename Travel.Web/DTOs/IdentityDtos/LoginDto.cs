using System.ComponentModel.DataAnnotations;

namespace Travel.Web.DTOs.IdentityDtos
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Lütfen e-posta adresinizi girin.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lütfen şifrenizi girin.")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}