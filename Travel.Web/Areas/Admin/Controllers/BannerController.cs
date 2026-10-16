using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Travel.Web.DTOs.BannerDtos;
using Travel.Web.Helpers;
using Travel.Web.Services.BannerServices;
using Travel.Web.Validations;

namespace Travel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Sadece rolü 'Admin' olan oturumlar girebilir
    public class BannerController(
        IBannerService _bannerService,
        IValidator<CreateBannerDto> _createValidator,
        IValidator<UpdateBannerDto> _updateValidator,
        IWebHostEnvironment _env,
        IStringLocalizer<SharedResource> _L) : Controller
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
        public async Task<IActionResult> Create(CreateBannerDto createBannerDto, IFormFile? imageFile)
        {
            // Dosya seçildiyse önce kontrol et; kaydetmeyi validasyon geçtikten sonra yap
            if (imageFile != null && !ImageUploadHelper.IsValid(imageFile))
                ModelState.AddModelError(nameof(CreateBannerDto.ImageUrl), _L["Sadece JPG, PNG, WEBP veya GIF (en fazla 5 MB) yükleyebilirsiniz."].Value);
            else if (imageFile != null)
                createBannerDto.ImageUrl = "/images/banners/pending"; // validator'ın "görsel boş" hatası vermemesi için geçici

            var result = await _createValidator.ValidateAsync(createBannerDto);
            result.AddToModelState(ModelState);
            if (!ModelState.IsValid)
            {
                createBannerDto.ImageUrl = imageFile != null ? null : createBannerDto.ImageUrl;
                return View(createBannerDto);
            }

            if (imageFile != null)
                createBannerDto.ImageUrl = await ImageUploadHelper.SaveAsync(imageFile, _env, "banners");

            await _bannerService.CreateAsync(createBannerDto);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Update(string id)
        {
            var banner = await _bannerService.GetByIdAsync(id);
            if (banner == null) return NotFound();

            return View(new UpdateBannerDto
            {
                Id = banner.Id,
                Title = banner.Title,
                Description = banner.Description,
                ImageUrl = banner.ImageUrl
            });
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateBannerDto updateBannerDto, IFormFile? imageFile)
        {
            var oldUrl = updateBannerDto.ImageUrl;

            if (imageFile != null && !ImageUploadHelper.IsValid(imageFile))
                ModelState.AddModelError(nameof(UpdateBannerDto.ImageUrl), _L["Sadece JPG, PNG, WEBP veya GIF (en fazla 5 MB) yükleyebilirsiniz."].Value);
            else if (imageFile != null)
                updateBannerDto.ImageUrl = "/images/banners/pending";

            var result = await _updateValidator.ValidateAsync(updateBannerDto);
            result.AddToModelState(ModelState);
            if (!ModelState.IsValid)
            {
                updateBannerDto.ImageUrl = oldUrl;
                return View(updateBannerDto);
            }

            if (imageFile != null)
                updateBannerDto.ImageUrl = await ImageUploadHelper.SaveAsync(imageFile, _env, "banners");

            await _bannerService.UpdateAsync(updateBannerDto);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string id)
        {
            await _bannerService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}