using Microsoft.AspNetCore.Mvc;
using Travel.Web.Services.DashboardServices;
using Travel.Web.Services.ReservationServices;
using Travel.Web.Services.CommentServices;
using Travel.Web.Services.QuestionServices;

namespace Travel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
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
            var statistics = await _dashboardService.GetDashboardStatisticsAsync();
            return View(statistics);
        }
    }
}