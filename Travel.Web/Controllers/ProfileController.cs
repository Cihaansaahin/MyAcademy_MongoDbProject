using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Travel.Web.DTOs.TourDtos;
using Travel.Web.Helpers;
using Travel.Web.Services.CommentServices;
using Travel.Web.Services.FavoriteServices;
using Travel.Web.Services.QuestionServices;
using Travel.Web.Services.ReservationServices;
using Travel.Web.Services.TourServices;
using Travel.Web.Services.UserServices;

namespace Travel.Web.Controllers
{
    // Giriş yapmamış kullanıcı otomatik olarak /Account/Login'e yönlendirilir (Program.cs LoginPath)
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly ICommentService _commentService;
        private readonly IQuestionService _questionService;
        private readonly IStringLocalizer<SharedResource> _L;
        private readonly IFavoriteService _favoriteService;
        private readonly ITourService _tourService;
        private readonly IUserService _userService;

        public ProfileController(
            IReservationService reservationService,
            ICommentService commentService,
            IQuestionService questionService,
            IStringLocalizer<SharedResource> localizer,
            IFavoriteService favoriteService,
            ITourService tourService,
            IUserService userService)
        {
            _reservationService = reservationService;
            _commentService = commentService;
            _questionService = questionService;
            _L = localizer;
            _favoriteService = favoriteService;
            _tourService = tourService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            // Kullanıcı bilgileri artık veritabanından okunuyor (Session'a bağlı değil)
            var userId = User.GetUserId();
            var user = string.IsNullOrEmpty(userId) ? null : await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                // Cookie var ama kullanıcı silinmiş vb. durum
                return RedirectToAction("Logout", "Account");
            }

            // Rezervasyonlar: yeni kayıtlar UserId ile, eski kayıtlar (UserId = e-posta) e-posta ile eşleşir
            var allReservations = await _reservationService.GetAllAsync();
            var userReservations = allReservations
                .Where(x => x.UserId == user.Id || x.Email == user.Email || x.UserId == user.Email)
                .OrderByDescending(x => x.ReservationDate)
                .ToList();

            // Yorum & Sorular: UserId ile (eski, UserId'siz kayıtlar için ad ile yedek eşleşme)
            var allComments = await _commentService.GetAllAsync();
            var userComments = allComments
                .Where(x => x.UserId == user.Id || (string.IsNullOrEmpty(x.UserId) && x.UserName == user.FullName))
                .ToList();

            var allQuestions = await _questionService.GetAllAsync();
            var userQuestions = allQuestions
                .Where(x => x.UserId == user.Id || (string.IsNullOrEmpty(x.UserId) && x.UserName == user.FullName))
                .ToList();

            ViewBag.Reservations = userReservations;
            ViewBag.Comments = userComments.OrderByDescending(x => x.CreatedDate).ToList();
            ViewBag.Questions = userQuestions;

            // Tüm turları bir kez çekip Id -> Tur sözlüğü yapıyoruz (yorum kartlarında tur adı için)
            var allTours = await _tourService.GetAllAsync();
            var tourMap = allTours.ToDictionary(x => x.Id, x => x);
            ViewBag.TourMap = tourMap;

            // Case Madde 9 & 11: Favori Turlarım (MongoDB Favorites koleksiyonundan)
            var favoriteIds = await _favoriteService.GetTourIdsByUserAsync(user.Id);
            var favoriteTours = favoriteIds
                .Where(tourMap.ContainsKey)          // Silinmiş tur favoride kalmışsa atla
                .Select(id => tourMap[id])
                .ToList();

            // Favori turların ortalama puanları (tek aggregation sorgusu)
            var ratings = await _commentService.GetRatingsForToursAsync(favoriteIds);
            var favoriteRatings = ratings.ToDictionary(x => x.Key, x => x.Value.AverageRating);

            ViewBag.FavoriteTours = favoriteTours;
            ViewBag.FavoriteRatings = favoriteRatings;

            ViewBag.UserName = user.FullName;
            ViewBag.FirstName = user.FirstName;
            ViewBag.LastName = user.LastName;
            ViewBag.UserEmail = user.Email;
            ViewBag.UserPhone = user.PhoneNumber;

            return View();
        }

        // Değerlendirmelerim: kullanıcının kendi yorumunun puanını günceller
        [HttpPost]
        public async Task<IActionResult> UpdateRating(string commentId, int rating)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(commentId) || rating < 1 || rating > 5)
            {
                return Json(new { success = false, message = _L["Geçersiz istek."].Value });
            }

            var ok = await _commentService.UpdateRatingAsync(commentId, userId, rating);
            return Json(new
            {
                success = ok,
                message = ok ? _L["Değerlendirmen güncellendi."].Value : _L["Geçersiz istek."].Value
            });
        }

        // Yorumlarım: kullanıcı kendi yorumunu siler
        [HttpPost]
        public async Task<IActionResult> DeleteComment(string commentId)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(commentId))
            {
                return Json(new { success = false, message = _L["Geçersiz istek."].Value });
            }

            var ok = await _commentService.DeleteByUserAsync(commentId, userId);
            return Json(new
            {
                success = ok,
                message = ok ? _L["Yorum silindi."].Value : _L["Geçersiz istek."].Value
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string firstName, string lastName, string email, string phone)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(email))
            {
                TempData["ProfileUpdateError"] = _L["Ad ve e-posta alanları zorunludur."].Value;
                return RedirectToAction("Index");
            }

            var result = await _userService.UpdateProfileAsync(userId, firstName, lastName ?? string.Empty, email, phone);

            if (result == UpdateProfileResult.EmailTaken)
            {
                TempData["ProfileUpdateError"] = _L["Bu e-posta adresiyle kayıtlı bir hesap zaten var."].Value;
                return RedirectToAction("Index");
            }

            if (result == UpdateProfileResult.NotFound)
            {
                return RedirectToAction("Logout", "Account");
            }

            // Cookie'deki ad/e-posta claim'lerini yenile ("Beni hatırla" seçimi korunur)
            var user = await _userService.GetByIdAsync(userId);
            var auth = await HttpContext.AuthenticateAsync();
            await HttpContext.SignInUserAsync(user!, auth.Properties?.IsPersistent ?? false);

            TempData["ProfileUpdateSuccess"] = _L["Profil bilgileriniz başarıyla güncellendi."].Value;
            return RedirectToAction("Index");
        }
    }
}