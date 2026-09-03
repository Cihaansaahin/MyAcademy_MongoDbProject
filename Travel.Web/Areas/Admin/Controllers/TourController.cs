using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Travel.Web.DTOs.TourDtos;
using Travel.Web.Services.CategoryServices;
using Travel.Web.Services.TourServices;

namespace Travel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TourController : Controller
    {
        private readonly ITourService _tourService;
        private readonly ICategoryService _categoryService;

        public TourController(ITourService tourService, ICategoryService categoryService)
        {
            _tourService = tourService;
            _categoryService = categoryService;
        }
        public async Task<IActionResult> Index()
        {
            var values = await _tourService.GetAllAsync();
            return View(values);
        }

        [HttpGet]
        public async Task<IActionResult> CreateTour()
        {
            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = categories.Select(x => new SelectListItem
            {
                Text = x.Name?.ToString() ?? "İsimsiz Kategori",
                Value = x.Id
            }).ToList();
            return View(new CreateTourDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateTour(CreateTourDto createTourDto)
        {
            // Formdan gelen Title.Value ve Description.Value değerlerini Türkçe (Tr) alanına da eşitleyelim
            if (createTourDto.Title != null)
            {
                createTourDto.Title.Tr = createTourDto.Title.Value ?? "";
            }

            if (createTourDto.Description != null)
            {
                createTourDto.Description.Tr = createTourDto.Description.Value ?? "";
            }

            // Listelerin null gitmesini engelleyelim (MongoDB boş array bekler)
            createTourDto.GalleryImageUrls ??= new();
            createTourDto.Features ??= new();
            createTourDto.TourDates ??= new();
            createTourDto.Itinerary ??= new();

            // Veritabanına kaydı tetikle
            await _tourService.CreateAsync(createTourDto);

            return RedirectToAction(nameof(Index));
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
                Value = x.Id
            }).ToList();

            // Mevcut tur verilerini düzenleme modeline aktarıyoruz
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
        public async Task<IActionResult> UpdateTour(UpdateTourDto updateTourDto)
        {
            updateTourDto.Title ??= new();
            updateTourDto.Description ??= new();

            updateTourDto.GalleryImageUrls ??= new();
            updateTourDto.Features ??= new();
            updateTourDto.TourDates ??= new();
            updateTourDto.Itinerary ??= new();

            await _tourService.UpdateAsync(updateTourDto);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteTour(string id)
        {
            await _tourService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
