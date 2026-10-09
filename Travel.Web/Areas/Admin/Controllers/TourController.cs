using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Travel.Web.DTOs.TourDtos;
using Travel.Web.Services.CategoryServices;
using Travel.Web.Services.DestinationServices;
using Travel.Web.Services.TourServices;
using Travel.Web.Validations;

namespace Travel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Sadece rolü 'Admin' olan oturumlar girebilir
    public class TourController : Controller
    {
        private readonly ITourService _tourService;
        private readonly ICategoryService _categoryService;
        private readonly IDestinationService _destinationService;
        private readonly IValidator<CreateTourDto> _createValidator;
        private readonly IValidator<UpdateTourDto> _updateValidator;

        public TourController(
            ITourService tourService,
            ICategoryService categoryService,
            IDestinationService destinationService,
            IValidator<CreateTourDto> createValidator,
            IValidator<UpdateTourDto> updateValidator)
        {
            _tourService = tourService;
            _categoryService = categoryService;
            _destinationService = destinationService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        // ================== YARDIMCI METOTLAR ==================

        // Kategori ve destinasyon dropdown'larını doldurur
        private async Task LoadDropdownsAsync(string? selectedCategoryId = null, string? selectedDestinationId = null)
        {
            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = categories.Select(x => new SelectListItem
            {
                Text = x.Name?.Tr ?? x.Name?.Value ?? "Kategori",
                Value = x.Id,
                Selected = x.Id == selectedCategoryId
            }).ToList();

            var destinations = await _destinationService.GetAllAsync();
            ViewBag.Destinations = destinations.Select(x => new SelectListItem
            {
                Text = $"{x.City}, {x.Country}",
                Value = x.Id,
                Selected = x.Id == selectedDestinationId
            }).ToList();
        }

        // Yüklenen görseli wwwroot/images/tours altına kaydeder, URL'sini döner
        private static async Task<string> SaveImageAsync(IFormFile file)
        {
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/tours");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/tours/{fileName}";
        }

        // ================== LİSTE ==================

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] TourFilterDto filter)
        {
            await LoadDropdownsAsync(filter.CategoryId, filter.DestinationId);
            ViewBag.CurrentFilter = filter;

            var tours = await _tourService.GetAllAsync();

            // Case Madde 17: Arama & Filtreleme
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var text = filter.SearchText.Trim().ToLower();
                tours = tours.Where(t =>
                    (t.Title?.Tr != null && t.Title.Tr.ToLower().Contains(text)) ||
                    (t.Title?.En != null && t.Title.En.ToLower().Contains(text)) ||
                    (t.City != null && t.City.ToLower().Contains(text)) ||
                    (t.Country != null && t.Country.ToLower().Contains(text))
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(filter.CategoryId))
                tours = tours.Where(t => t.CategoryId == filter.CategoryId).ToList();

            if (!string.IsNullOrWhiteSpace(filter.DestinationId))
                tours = tours.Where(t => t.DestinationId == filter.DestinationId).ToList();

            if (filter.IsActive.HasValue)
                tours = tours.Where(t => t.IsActive == filter.IsActive.Value).ToList();

            return View(tours);
        }

        // ================== EKLEME ==================

        [HttpGet]
        public async Task<IActionResult> CreateTour()
        {
            await LoadDropdownsAsync();
            return View(new CreateTourDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateTour(CreateTourDto dto)
        {
            // 1. Validasyon — dosya kaydetmeden ÖNCE
            var validation = await _createValidator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                validation.AddToModelState(ModelState);
                await LoadDropdownsAsync(dto.CategoryId, dto.DestinationId);
                return View(dto);
            }

            // 2. Kapak görseli
            if (dto.CoverImageFile != null && dto.CoverImageFile.Length > 0)
                dto.CoverImageUrl = await SaveImageAsync(dto.CoverImageFile);

            // 3. Galeri görselleri
            if (dto.GalleryFiles != null && dto.GalleryFiles.Any())
            {
                dto.GalleryImageUrls = new List<string>();
                foreach (var file in dto.GalleryFiles.Where(f => f.Length > 0))
                    dto.GalleryImageUrls.Add(await SaveImageAsync(file));
            }

            await _tourService.CreateAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        // ================== GÜNCELLEME ==================

        [HttpGet]
        public async Task<IActionResult> UpdateTour(string id)
        {
            var tour = await _tourService.GetByIdAsync(id);
            if (tour == null)
                return NotFound();

            await LoadDropdownsAsync(tour.CategoryId, tour.DestinationId);

            var model = new UpdateTourDto
            {
                Id = tour.Id,
                Title = tour.Title,
                Description = tour.Description,
                Price = tour.Price,
                DurationDays = tour.DurationDays,
                Country = tour.Country,
                City = tour.City,
                CategoryId = tour.CategoryId,
                DestinationId = tour.DestinationId,
                CoverImageUrl = tour.CoverImageUrl,
                GalleryImageUrls = tour.GalleryImageUrls,
                Features = tour.Features,
                IsActive = tour.IsActive,
                IsPopular = tour.IsPopular,
                TourDates = tour.TourDates,
                Itinerary = tour.Itinerary
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTour(UpdateTourDto dto)
        {
            var existing = await _tourService.GetByIdAsync(dto.Id);
            if (existing == null)
                return NotFound();

            // 1. Validasyon — dosya kaydetmeden ÖNCE
            var validation = await _updateValidator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                validation.AddToModelState(ModelState);
                await LoadDropdownsAsync(dto.CategoryId, dto.DestinationId);

                // Form kapak önizlemesini gösterebilsin
                if (string.IsNullOrWhiteSpace(dto.CoverImageUrl))
                    dto.CoverImageUrl = existing.CoverImageUrl;

                return View(dto);
            }

            // 2. Kapak görseli: yeni yüklendiyse kaydet, yüklenmediyse ESKİSİNİ KORU
            if (dto.CoverImageFile != null && dto.CoverImageFile.Length > 0)
                dto.CoverImageUrl = await SaveImageAsync(dto.CoverImageFile);
            else if (string.IsNullOrWhiteSpace(dto.CoverImageUrl))
                dto.CoverImageUrl = existing.CoverImageUrl;

            // 3. Galeri: yeni yüklendiyse kaydet, yüklenmediyse eskileri koru
            if (dto.GalleryFiles != null && dto.GalleryFiles.Any())
            {
                dto.GalleryImageUrls ??= new List<string>();
                foreach (var file in dto.GalleryFiles.Where(f => f.Length > 0))
                    dto.GalleryImageUrls.Add(await SaveImageAsync(file));
            }
            else
            {
                dto.GalleryImageUrls = existing.GalleryImageUrls ?? new List<string>();
            }

            await _tourService.UpdateAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        // ================== SİLME ==================

        public async Task<IActionResult> DeleteTour(string id)
        {
            await _tourService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}