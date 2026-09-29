using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel.Web.DTOs.QuestionDtos;
using Travel.Web.Services.QuestionServices;

namespace Travel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Sadece rolü 'Admin' olan oturumlar girebilir
    public class QuestionController : Controller
    {
        private readonly IQuestionService _questionService;

        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        public async Task<IActionResult> Index()
        {
            var values = await _questionService.GetAllAsync();
            return View(values);
        }

        [HttpGet]
        public async Task<IActionResult> AnswerQuestion(string id)
        {
            var value = await _questionService.GetByIdAsync(id);
            if (value == null)
            {
                return NotFound();
            }

            var model = new AnswerQuestionDto
            {
                QuestionId = value.Id,
                AnswerText = value.AnswerText ?? string.Empty
            };

            ViewBag.UserName = value.UserName;
            ViewBag.QuestionText = value.QuestionText;
            ViewBag.AskedDate = value.AskedDate;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AnswerQuestion(AnswerQuestionDto answerQuestionDto)
        {
            await _questionService.AnswerQuestionAsync(answerQuestionDto);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string id)
        {
            await _questionService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}