using Microsoft.AspNetCore.Mvc;
using Travel.Web.Services.TourServices;
using Travel.Web.Services.DestinationServices;

namespace Travel.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITourService _tourService;
        private readonly IDestinationService _destinationService;

        public HomeController(ITourService tourService, IDestinationService destinationService)
        {
            _tourService = tourService;
            _destinationService = destinationService;
        }

        public async Task<IActionResult> Index()
        {
            var tours = await _tourService.GetAllAsync();
            var destinations = await _destinationService.GetAllAsync();

            ViewBag.Destinations = destinations;
            return View(tours);
        }
    }
}