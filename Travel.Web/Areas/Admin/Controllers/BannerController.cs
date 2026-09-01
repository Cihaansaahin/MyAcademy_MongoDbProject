using Microsoft.AspNetCore.Mvc;
using Travel.Web.DTOs.BannerDtos;
using Travel.Web.Services.BannerServices;

namespace Travel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BannerController(IBannerService _bannerService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var banners = await _bannerService.GetAllAsync();
            return View(banners);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateBannerDto createBannerDto)
        {
            if (!ModelState.IsValid)
            {
                return View(createBannerDto);
            }
            await _bannerService.CreateAsync(createBannerDto);
            return RedirectToAction(nameof(Index));

        }
    }
}
