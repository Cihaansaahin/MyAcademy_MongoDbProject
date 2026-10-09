using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel.Web.DTOs.BannerDtos;
using Travel.Web.Services.BannerServices;
using Travel.Web.Validations;

namespace Travel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BannerController(IBannerService _bannerService, IValidator<CreateBannerDto> _validator) : Controller
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
            var validation = await _validator.ValidateAsync(createBannerDto);
            if (!validation.IsValid)
            {
                validation.AddToModelState(ModelState);
                return View(createBannerDto);
            }

            await _bannerService.CreateAsync(createBannerDto);
            return RedirectToAction(nameof(Index));
        }
    }
}