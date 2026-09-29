using System.ComponentModel.DataAnnotations;

namespace Travel.Web.DTOs.IdentityDtos
{
    public class CreateRegisterDto
    {
        [Required(ErrorMessage = "Lütfen adınızı girin.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lütfen soyadınızı girin.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        [EmailAddress(ErrorMessage = "E-posta formatı geçersiz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lütfen telefon numaranızı girin.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [MinLength(8, ErrorMessage = "Şifre en az 8 karakter olmalıdır.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool AgreeTerms { get; set; }
    }
}