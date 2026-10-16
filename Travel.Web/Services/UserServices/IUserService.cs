using Travel.Web.DTOs.IdentityDtos;
using Travel.Web.Entities;

namespace Travel.Web.Services.UserServices
{
    public enum UpdateProfileResult
    {
        Success,
        NotFound,
        EmailTaken
    }

    public interface IUserService
    {
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByEmailAsync(string email);

        // E-posta zaten kayıtlıysa null döner
        Task<User?> RegisterAsync(CreateRegisterDto dto);

        // E-posta veya şifre yanlışsa null döner
        Task<User?> ValidateCredentialsAsync(string email, string password);

        Task<long> CountMembersAsync();

        // Profil bilgilerini günceller (Ad, Soyad, E-posta, Telefon)
        Task<UpdateProfileResult> UpdateProfileAsync(string userId, string firstName, string lastName, string email, string phone);

        // Uygulama açılırken: e-posta için unique index + admin hesabı yoksa oluştur
        Task InitializeAsync(string adminEmail, string adminPassword);
    }
}