using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using Travel.Web.DTOs.IdentityDtos;
using Travel.Web.Entities;
using Travel.Web.Settings;

namespace Travel.Web.Services.UserServices
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _userCollection;

        // ASP.NET Core'un kendi şifre hash'leyicisi (PBKDF2 + salt). Ek paket gerekmez.
        private readonly PasswordHasher<User> _hasher = new();

        public UserService(IDataBaseSettings databaseSettings)
        {
            var client = new MongoClient(databaseSettings.ConnectionString);
            var database = client.GetDatabase(databaseSettings.DataBaseName);
            _userCollection = database.GetCollection<User>(databaseSettings.UserCollectionName);
        }

        private static string Normalize(string email) => (email ?? string.Empty).Trim().ToLowerInvariant();

        public async Task<User?> GetByIdAsync(string id)
        {
            return await _userCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var normalized = Normalize(email);
            return await _userCollection.Find(x => x.Email == normalized).FirstOrDefaultAsync();
        }

        public async Task<User?> RegisterAsync(CreateRegisterDto dto)
        {
            if (await GetByEmailAsync(dto.Email) != null)
                return null; // Bu e-posta ile zaten hesap var

            var user = new User
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = Normalize(dto.Email),
                PhoneNumber = dto.PhoneNumber.Trim(),
                Role = "Member",
                CreatedDate = DateTime.UtcNow
            };
            user.PasswordHash = _hasher.HashPassword(user, dto.Password);

            await _userCollection.InsertOneAsync(user);
            return user;
        }

        public async Task<User?> ValidateCredentialsAsync(string email, string password)
        {
            var user = await GetByEmailAsync(email);
            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                return null;

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Failed ? null : user;
        }

        public async Task<long> CountMembersAsync()
        {
            return await _userCollection.CountDocumentsAsync(x => x.Role == "Member");
        }

        public async Task<UpdateProfileResult> UpdateProfileAsync(string userId, string firstName, string lastName, string email, string phone)
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
                return UpdateProfileResult.NotFound;

            var normalized = Normalize(email);

            // E-posta değişiyorsa başka bir hesapta kullanılıyor mu?
            if (normalized != user.Email)
            {
                var other = await GetByEmailAsync(normalized);
                if (other != null && other.Id != user.Id)
                    return UpdateProfileResult.EmailTaken;
            }

            var update = Builders<User>.Update
                .Set(x => x.FirstName, firstName.Trim())
                .Set(x => x.LastName, lastName.Trim())
                .Set(x => x.Email, normalized)
                .Set(x => x.PhoneNumber, (phone ?? string.Empty).Trim());

            await _userCollection.UpdateOneAsync(x => x.Id == userId, update);
            return UpdateProfileResult.Success;
        }

        public async Task InitializeAsync(string adminEmail, string adminPassword)
        {
            // Aynı e-posta ile iki hesap açılamasın (veritabanı seviyesinde garanti)
            var indexModel = new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(x => x.Email),
                new CreateIndexOptions { Unique = true });
            await _userCollection.Indexes.CreateOneAsync(indexModel);

            // Admin yoksa oluştur
            if (await GetByEmailAsync(adminEmail) != null)
                return;

            var admin = new User
            {
                FirstName = "Admin",
                LastName = string.Empty,
                Email = Normalize(adminEmail),
                Role = "Admin",
                CreatedDate = DateTime.UtcNow
            };
            admin.PasswordHash = _hasher.HashPassword(admin, adminPassword);
            await _userCollection.InsertOneAsync(admin);
        }
    }
}