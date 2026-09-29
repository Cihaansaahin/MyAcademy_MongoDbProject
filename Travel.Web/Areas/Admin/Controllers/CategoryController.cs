using Microsoft.AspNetCore.Mvc;
using Travel.Web.DTOs.CategoryDtos;
using Travel.Web.Services.CategoryServices;

namespace Travel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var values = await _categoryService.GetAllAsync();
            return View(values);
        }

        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CreateCategoryDto createCategoryDto)
        {
            createCategoryDto.Name ??= new();

            if (string.IsNullOrWhiteSpace(createCategoryDto.Name.En))
                createCategoryDto.Name.En = createCategoryDto.Name.Tr;
            if (string.IsNullOrWhiteSpace(createCategoryDto.Name.Tr))
                createCategoryDto.Name.Tr = createCategoryDto.Name.En;

            await _categoryService.CreateAsync(createCategoryDto);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteCategory(string id)
        {
            await _categoryService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateCategory(string id)
        {
            var value = await _categoryService.GetByIdAsync(id);
            if (value == null)
            {
                return NotFound();
            }

            var model = new UpdateCategoryDto
            {
                Id = value.Id,
                Name = value.Name,
                IconUrl = value.IconUrl,
                IsActive = value.IsActive
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory(UpdateCategoryDto updateCategoryDto)
        {
            updateCategoryDto.Name ??= new();

            if (string.IsNullOrWhiteSpace(updateCategoryDto.Name.En))
                updateCategoryDto.Name.En = updateCategoryDto.Name.Tr;
            if (string.IsNullOrWhiteSpace(updateCategoryDto.Name.Tr))
                updateCategoryDto.Name.Tr = updateCategoryDto.Name.En;

            await _categoryService.UpdateAsync(updateCategoryDto);
            return RedirectToAction(nameof(Index));
        }
    }
}