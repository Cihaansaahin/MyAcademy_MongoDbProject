using Microsoft.AspNetCore.Mvc;
using Travel.Web.DTOs.DestinationDtos;
using Travel.Web.Services.DestinationServices;

namespace Travel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DestinationController : Controller
    {
        private readonly IDestinationService _destinationService;

        public DestinationController(IDestinationService destinationService)
        {
            _destinationService = destinationService;
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
        public async Task<IActionResult> CreateDestination(CreateDestinationDto createDestinationDto)
        {
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
        public async Task<IActionResult> UpdateDestination(UpdateDestinationDto updateDestinationDto)
        {
            await _destinationService.UpdateAsync(updateDestinationDto);
            return RedirectToAction(nameof(Index));
        }
    }
}