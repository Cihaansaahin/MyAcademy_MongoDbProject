using Microsoft.AspNetCore.Mvc;
using Travel.Web.Helpers;
using Travel.Web.Services.BannerServices;
using Travel.Web.Services.CommentServices;
using Travel.Web.Services.DashboardServices;
using Travel.Web.Services.DestinationServices;
using Travel.Web.Services.FavoriteServices;
using Travel.Web.Services.TourServices;
using Travel.Web.Services.UserServices;

namespace Travel.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITourService _tourService;
        private readonly IDestinationService _destinationService;
        private readonly ICommentService _commentService;
        private readonly IUserService _userService;
        private readonly IDashboardService _dashboardService;
        private readonly IFavoriteService _favoriteService;
        private readonly IBannerService _bannerService;

        public HomeController(
            ITourService tourService,
            IDestinationService destinationService,
            ICommentService commentService,
            IUserService userService,
            IDashboardService dashboardService,
            IFavoriteService favoriteService,
            IBannerService bannerService)
        {
            _tourService = tourService;
            _destinationService = destinationService;
            _commentService = commentService;
            _userService = userService;
            _dashboardService = dashboardService;
            _favoriteService = favoriteService;
            _bannerService = bannerService;
        }

        public async Task<IActionResult> Index()
        {
            var allTours = await _tourService.GetAllAsync();
            var activeTours = allTours.Where(x => x.IsActive).ToList();
            var destinations = await _destinationService.GetAllAsync();

            // 1. En çok tercih edilen 4 tur: rezervasyon sayısına göre (Aggregation), eşitse "Popüler" işaretliler önde
            var bookingCounts = await _dashboardService.GetBookingCountsByTourAsync();
            var topTours = activeTours
                .OrderByDescending(t => bookingCounts.TryGetValue(t.Id, out var c) ? c : 0)
                .ThenByDescending(t => t.IsPopular)
                .Take(4)
                .ToList();

            ViewBag.BookingCounts = bookingCounts;
            ViewBag.TourRatings = await _commentService.GetRatingsForToursAsync(topTours.Select(x => x.Id));

            // 2. Hero istatistikleri (gerçek veriler)
            ViewBag.TotalTours = activeTours.Count;
            ViewBag.TotalDestinations = destinations.Count;
            ViewBag.TotalUsers = await _userService.CountMembersAsync();

            // 3. Popüler destinasyonlar: "Popüler" işaretliler önce, sonra en çok turu olanlar
            var destTourCounts = activeTours
                .Where(t => !string.IsNullOrEmpty(t.DestinationId))
                .GroupBy(t => t.DestinationId)
                .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.Destinations = destinations
                .OrderByDescending(d => d.IsPopular)
                .ThenByDescending(d => destTourCounts.TryGetValue(d.Id, out var c) ? c : 0)
                .Take(4)
                .ToList();
            ViewBag.DestTourCounts = destTourCounts;

            // 4. Gezgin yorumları: son 3 onaylı, 4+ yıldızlı gerçek yorum
            var comments = await _commentService.GetAllAsync();
            ViewBag.Reviews = comments
                .Where(x => x.IsApproved && x.Rating >= 4 && !string.IsNullOrWhiteSpace(x.Content))
                .OrderByDescending(x => x.CreatedDate)
                .Take(3)
                .ToList();
            ViewBag.TourTitles = allTours.ToDictionary(x => x.Id, x => x.Title?.Value ?? string.Empty);

            // 5. Hero slider: admin panelinden eklenen bannerlar (yoksa varsayılan görsel/metin)
            ViewBag.Banners = await _bannerService.GetAllAsync();

            // 6. Kart kalpleri için kullanıcının favorileri
            var userId = User.GetUserId();
            ViewBag.FavoriteIds = string.IsNullOrEmpty(userId)
                ? new HashSet<string>()
                : (await _favoriteService.GetTourIdsByUserAsync(userId)).ToHashSet();

            return View(topTours);
        }
    }
}