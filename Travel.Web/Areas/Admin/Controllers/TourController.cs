using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Travel.Web.DTOs.TourDtos;
using Travel.Web.Services.CategoryServices;
using Travel.Web.Services.DestinationServices;
using Travel.Web.Services.TourServices;

namespace Travel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Sadece rolü 'Admin' olan oturumlar girebilir
    public class TourController : Controller
    {
        private readonly ITourService _tourService;
        private readonly ICategoryService _categoryService;
        private readonly IDestinationService _destinationService;

        public TourController(
            ITourService tourService,
            ICategoryService categoryService,
            IDestinationService destinationService)
        {
            _tourService = tourService;
            _categoryService = categoryService;
            _destinationService = destinationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] TourFilterDto filter)
        {
            // Dropdown filtreleri için verileri çekiyoruz
            var categories = await _categoryService.GetAllAsync();
            var destinations = await _destinationService.GetAllAsync();

            ViewBag.Categories = categories.Select(x => new SelectListItem
            {
                Text = x.Name?.Tr ?? x.Name?.Value ?? "Kategori",
                Value = x.Id,
                Selected = x.Id == filter.CategoryId
            }).ToList();

            ViewBag.Destinations = destinations.Select(x => new SelectListItem
            {
                Text = $"{x.City}, {x.Country}",
                Value = x.Id,
                Selected = x.Id == filter.DestinationId
            }).ToList();

            ViewBag.CurrentFilter = filter;

            var tours = await _tourService.GetAllAsync();

            // Case Madde 17: Arama & Filtreleme Mantığı
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var text = filter.SearchText.Trim().ToLower();
                tours = tours.Where(t =>
                    (t.Title?.Tr != null && t.Title.Tr.ToLower().Contains(text)) ||
                    (t.Title?.En != null && t.Title.En.ToLower().Contains(text)) ||
                    (t.Title?.Value != null && t.Title.Value.ToLower().Contains(text)) ||
                    (t.City != null && t.City.ToLower().Contains(text)) ||
                    (t.Country != null && t.Country.ToLower().Contains(text))
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(filter.CategoryId))
            {
                tours = tours.Where(t => t.CategoryId == filter.CategoryId).ToList();
            }

            if (!string.IsNullOrWhiteSpace(filter.DestinationId))
            {
                tours = tours.Where(t => t.DestinationId == filter.DestinationId).ToList();
            }

            if (filter.IsActive.HasValue)
            {
                tours = tours.Where(t => t.IsActive == filter.IsActive.Value).ToList();
            }

            return View(tours);
        }

        [HttpGet]
        public async Task<IActionResult> CreateTour()
        {
            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = categories.Select(x => new SelectListItem
            {
                Text = x.Name?.Tr ?? x.Name?.Value ?? "İsimsiz Kategori",
                Value = x.Id
            }).ToList();

            var destinations = await _destinationService.GetAllAsync();
            ViewBag.Destinations = destinations.Select(x => new SelectListItem
            {
                Text = $"{x.City}, {x.Country}",
                Value = x.Id
            }).ToList();

            return View(new CreateTourDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateTour(CreateTourDto dto)
        {
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/tours");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            // 1. Kapak fotoğrafı dosyadan yüklendiyse kaydet
            if (dto.CoverImageFile != null && dto.CoverImageFile.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.CoverImageFile.FileName)}";
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.CoverImageFile.CopyToAsync(stream);
                }
                dto.CoverImageUrl = $"/images/tours/{fileName}";
            }

            // 2. Galeri fotoğrafları yüklendiyse tek tek kaydet ve listeye ekle
            if (dto.GalleryFiles != null && dto.GalleryFiles.Any())
            {
                dto.GalleryImageUrls = new List<string>();

                foreach (var file in dto.GalleryFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var filePath = Path.Combine(folder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        dto.GalleryImageUrls.Add($"/images/tours/{fileName}");
                    }
                }
            }

            await _tourService.CreateAsync(dto);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateTour(string id)
        {
            var tour = await _tourService.GetByIdAsync(id);
            if (tour == null)
            {
                return NotFound();
            }

            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = categories.Select(x => new SelectListItem
            {
                Text = x.Name?.Tr ?? x.Name?.Value ?? "Kategori",
                Value = x.Id,
                Selected = x.Id == tour.CategoryId
            }).ToList();

            var destinations = await _destinationService.GetAllAsync();
            ViewBag.Destinations = destinations.Select(x => new SelectListItem
            {
                Text = $"{x.City}, {x.Country}",
                Value = x.Id,
                Selected = x.Id == tour.DestinationId
            }).ToList();

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
            // 1. Kapak fotoğrafı dosyadan yüklendiyse kaydet
            if (dto.CoverImageFile != null && dto.CoverImageFile.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/tours");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.CoverImageFile.FileName)}";
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.CoverImageFile.CopyToAsync(stream);
                }
                dto.CoverImageUrl = $"/images/tours/{fileName}";
            }

            // 2. Galeri fotoğrafları yüklendiyse kaydet
            if (dto.GalleryFiles != null && dto.GalleryFiles.Any())
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/tours");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                dto.GalleryImageUrls ??= new List<string>();

                foreach (var file in dto.GalleryFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var filePath = Path.Combine(folder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        dto.GalleryImageUrls.Add($"/images/tours/{fileName}");
                    }
                }
            }
            else
            {
                // Yeni galeri yüklenmediyse eski fotoğrafları korumak için:
                var existing = await _tourService.GetByIdAsync(dto.Id);
                if (existing?.GalleryImageUrls != null)
                {
                    dto.GalleryImageUrls = existing.GalleryImageUrls;
                }
            }

            await _tourService.UpdateAsync(dto);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteTour(string id)
        {
            await _tourService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}