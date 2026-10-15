using Travel.Web.DTOs.IdentityDtos;
using Travel.Web.Entities;

namespace Travel.Web.Services.UserServices
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByEmailAsync(string email);

       
        Task<User?> RegisterAsync(CreateRegisterDto dto);

        Task<User?> ValidateCredentialsAsync(string email, string password);

        Task<long> CountMembersAsync();

       
        Task InitializeAsync(string adminEmail, string adminPassword);
    }
}