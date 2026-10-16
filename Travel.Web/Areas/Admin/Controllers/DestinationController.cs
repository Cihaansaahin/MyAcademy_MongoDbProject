using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel.Web.DTOs.DestinationDtos;
using Travel.Web.Helpers;
using Travel.Web.Services.DestinationServices;

namespace Travel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Sadece rolü 'Admin' olan oturumlar girebilir
    public class DestinationController : Controller
    {
        private readonly IDestinationService _destinationService;
        private readonly IWebHostEnvironment _env;

        public DestinationController(IDestinationService destinationService, IWebHostEnvironment env)
        {
            _destinationService = destinationService;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var values = await _destinationService.GetAllAsync();
            return View(values);
        }

        [HttpGet]
        public IActionResult CreateDestination()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateDestination(CreateDestinationDto createDestinationDto, IFormFile? imageFile)
        {
            // Dosya seçildiyse URL alanı yerine yüklenen görsel kullanılır
            if (ImageUploadHelper.IsValid(imageFile))
            {
                createDestinationDto.ImageUrl = await ImageUploadHelper.SaveAsync(imageFile!, _env, "destinations");
            }

            await _destinationService.CreateAsync(createDestinationDto);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteDestination(string id)
        {
            await _destinationService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateDestination(string id)
        {
            var value = await _destinationService.GetByIdAsync(id);
            if (value == null)
            {
                return NotFound();
            }

            var model = new UpdateDestinationDto
            {
                Id = value.Id,
                City = value.City,
                Country = value.Country,
                ImageUrl = value.ImageUrl,
                IsPopular = value.IsPopular
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDestination(UpdateDestinationDto updateDestinationDto, IFormFile? imageFile)
        {
            // Yeni dosya seçildiyse onu kullan; seçilmediyse formdaki mevcut URL korunur
            if (ImageUploadHelper.IsValid(imageFile))
            {
                updateDestinationDto.ImageUrl = await ImageUploadHelper.SaveAsync(imageFile!, _env, "destinations");
            }

            await _destinationService.UpdateAsync(updateDestinationDto);
            return RedirectToAction(nameof(Index));
        }
    }
}