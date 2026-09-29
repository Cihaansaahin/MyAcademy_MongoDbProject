using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Travel.Web.DTOs.TourDtos;
using Travel.Web.Services.CategoryServices;
using Travel.Web.Services.DestinationServices;
using Travel.Web.Services.TourServices;

namespace Travel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
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
        public async Task<IActionResult> CreateTour(CreateTourDto createTourDto)
        {
            if (createTourDto.Title != null)
            {
                createTourDto.Title.Tr = createTourDto.Title.Value ?? createTourDto.Title.Tr ?? "";
            }

            if (createTourDto.Description != null)
            {
                createTourDto.Description.Tr = createTourDto.Description.Value ?? createTourDto.Description.Tr ?? "";
            }

            createTourDto.GalleryImageUrls ??= new();
            createTourDto.Features ??= new();
            createTourDto.TourDates ??= new();
            createTourDto.Itinerary ??= new();

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