using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel.Web.Services.CommentServices;
using Travel.Web.Services.DashboardServices;
using Travel.Web.Services.QuestionServices;
using Travel.Web.Services.ReservationServices;

namespace Travel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly IReservationService _reservationService;
        private readonly ICommentService _commentService;
        private readonly IQuestionService _questionService;

        public DashboardController(
            IDashboardService dashboardService,
            IReservationService reservationService,
            ICommentService commentService,
            IQuestionService questionService)
        {
            _dashboardService = dashboardService;
            _reservationService = reservationService;
            _commentService = commentService;
            _questionService = questionService;
        }

   
        public async Task<IActionResult> Index()
        {
            // 1. Genel İstatistikler
            var statistics = await _dashboardService.GetDashboardStatisticsAsync();

            // 2. Son Rezervasyonlar
            var allReservations = await _reservationService.GetAllAsync();
            ViewBag.RecentReservations = allReservations?
                .OrderByDescending(x => x.ReservationDate)
                .Take(5)
                .ToList();

            // 3. Son Yorumlar (Filtresiz direkt son eklenenler)
            var allComments = await _commentService.GetAllAsync();
            ViewBag.RecentComments = allComments?
                .OrderByDescending(x => x.CreatedDate)
                .Take(5)
                .ToList();

            // 4. Yanıt Bekleyen Sorular (Cevabı olmayan tüm sorular)
            var allQuestions = await _questionService.GetAllAsync();
            ViewBag.PendingQuestions = allQuestions?
                .Where(x => string.IsNullOrEmpty(x.AnswerText))
                .OrderByDescending(x => x.AskedDate) // AskedDate ile sıralıyoruz
                .Take(5)
                .ToList();

            return View(statistics);
        }
    }
}