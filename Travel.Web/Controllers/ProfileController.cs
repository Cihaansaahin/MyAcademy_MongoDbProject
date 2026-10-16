using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using Travel.Web.DTOs.TourDtos;
using Travel.Web.Services.FavoriteServices;
using Travel.Web.Services.TourServices;
using Travel.Web.Services.CommentServices;
using Travel.Web.Services.QuestionServices;
using Travel.Web.Services.ReservationServices;

namespace Travel.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly ICommentService _commentService;
        private readonly IQuestionService _questionService;
        private readonly IStringLocalizer<SharedResource> _L;
        private readonly IFavoriteService _favoriteService;
        private readonly ITourService _tourService;

        public ProfileController(
            IReservationService reservationService,
            ICommentService commentService,
            IQuestionService questionService,
            IStringLocalizer<SharedResource> localizer,
            IFavoriteService favoriteService,
            ITourService tourService)
        {
            _reservationService = reservationService;
            _commentService = commentService;
            _questionService = questionService;
            _L = localizer;
            _favoriteService = favoriteService;
            _tourService = tourService;
        }

        public async Task<IActionResult> Index()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userName = HttpContext.Session.GetString("UserName");

            // Oturum kontrolü: Giriş yapılmamışsa doğrudan Login sayfasına yönlendir
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            // Tüm verileri çekip oturumdaki kullanıcıya göre filtreliyoruz
            var allReservations = await _reservationService.GetAllAsync();
            var allComments = await _commentService.GetAllAsync();
            var allQuestions = await _questionService.GetAllAsync();

            // Sadece bu kullanıcıya ait olanlar
            var userReservations = allReservations
                .Where(x => x.Email == userEmail || x.UserId == userEmail)
                .OrderByDescending(x => x.ReservationDate)
                .ToList();

            var userComments = allComments
                .Where(x => x.UserName == userName)
                .ToList();

            var userQuestions = allQuestions
                .Where(x => x.UserName == userName)
                .ToList();

            ViewBag.Reservations = userReservations;
            ViewBag.Comments = userComments;
            ViewBag.Questions = userQuestions;

            // Case Madde 9 & 11: Favori Turlarım (MongoDB Favorites koleksiyonundan)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favoriteTours = new List<ResultTourDto>();
            var favoriteRatings = new Dictionary<string, double>();

            if (!string.IsNullOrEmpty(userId))
            {
                var favoriteIds = await _favoriteService.GetTourIdsByUserAsync(userId);
                foreach (var tourId in favoriteIds)
                {
                    var tour = await _tourService.GetByIdAsync(tourId);
                    if (tour == null) continue; // Silinmiş tur favoride kalmışsa atla

                    favoriteTours.Add(tour);
                    var summary = await _commentService.GetTourRatingSummaryAsync(tourId);
                    favoriteRatings[tourId] = summary.AverageRating;
                }
            }

            ViewBag.FavoriteTours = favoriteTours;
            ViewBag.FavoriteRatings = favoriteRatings;

            ViewBag.UserName = userName ?? "Gezgin Kullanıcı";
            ViewBag.UserEmail = userEmail;
            ViewBag.UserPhone = HttpContext.Session.GetString("UserPhone") ?? "+90 555 123 45 67";

            return View();
        }


        [HttpPost]
        public IActionResult UpdateProfile(string fullName, string email, string phone)
        {
            if (!string.IsNullOrWhiteSpace(fullName))
                HttpContext.Session.SetString("UserName", fullName.Trim());

            if (!string.IsNullOrWhiteSpace(email))
                HttpContext.Session.SetString("UserEmail", email.Trim());

            if (!string.IsNullOrWhiteSpace(phone))
                HttpContext.Session.SetString("UserPhone", phone.Trim());

            TempData["ProfileUpdateSuccess"] = _L["Profil bilgileriniz başarıyla güncellendi."].Value;
            return RedirectToAction("Index");
        }
    }
}