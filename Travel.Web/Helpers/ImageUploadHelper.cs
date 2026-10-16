namespace Travel.Web.Helpers
{
    // Admin panelinde yüklenen görselleri wwwroot/images/{klasör} altına kaydeder
    public static class ImageUploadHelper
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

        public static bool IsValid(IFormFile? file)
        {
            if (file == null || file.Length == 0) return false;
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedExtensions.Contains(ext) && file.Length <= MaxBytes;
        }

        // Kaydedilen dosyanın site içi yolunu döner: "/images/banners/abc123.jpg"
        public static async Task<string> SaveAsync(IFormFile file, IWebHostEnvironment env, string folder)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var dir = Path.Combine(env.WebRootPath, "images", folder);
            Directory.CreateDirectory(dir);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            await using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/{folder}/{fileName}";
        }
    }
}