using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel.Web.DTOs.QuestionDtos;
using Travel.Web.Services.QuestionServices;
using Travel.Web.Validations;

namespace Travel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class QuestionController : Controller
    {
        private readonly IQuestionService _questionService;
        private readonly IValidator<AnswerQuestionDto> _answerValidator;

        public QuestionController(IQuestionService questionService, IValidator<AnswerQuestionDto> answerValidator)
        {
            _questionService = questionService;
            _answerValidator = answerValidator;
        }

        public async Task<IActionResult> Index()
        {
            var values = await _questionService.GetAllAsync();
            return View(values);
        }

        // Sayfanın üstündeki "soru kartı" bilgilerini doldurur (GET ve hatalı POST için ortak)
        private async Task<bool> LoadQuestionInfoAsync(string id)
        {
            var question = await _questionService.GetByIdAsync(id);
            if (question == null)
                return false;

            ViewBag.UserName = question.UserName;
            ViewBag.QuestionText = question.QuestionText;
            ViewBag.AskedDate = question.AskedDate;
            return true;
        }

        [HttpGet]
        public async Task<IActionResult> AnswerQuestion(string id)
        {
            var value = await _questionService.GetByIdAsync(id);
            if (value == null)
                return NotFound();

            await LoadQuestionInfoAsync(id);

            var model = new AnswerQuestionDto
            {
                QuestionId = value.Id,
                AnswerText = value.AnswerText ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AnswerQuestion(AnswerQuestionDto answerQuestionDto)
        {
            var validation = await _answerValidator.ValidateAsync(answerQuestionDto);
            if (!validation.IsValid)
            {
                validation.AddToModelState(ModelState);

                if (!await LoadQuestionInfoAsync(answerQuestionDto.QuestionId))
                    return NotFound();

                return View(answerQuestionDto);
            }

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