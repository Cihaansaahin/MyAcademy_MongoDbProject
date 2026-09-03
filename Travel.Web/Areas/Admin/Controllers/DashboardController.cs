using Microsoft.AspNetCore.Mvc;
using Travel.Web.Services.DashboardServices;

namespace Travel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var statistics = await _dashboardService.GetDashboardStatisticsAsync();
            return View(statistics);
        }
    }
}