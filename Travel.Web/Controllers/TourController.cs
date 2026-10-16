using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Travel.Web.DTOs.CommentDtos;
using Travel.Web.DTOs.QuestionDtos;
using Travel.Web.DTOs.ReservationDtos;
using Travel.Web.Services.CategoryServices;
using Travel.Web.Services.CommentServices;
using Travel.Web.Services.DestinationServices;
using Travel.Web.Services.QuestionServices;
using Travel.Web.Services.ReservationServices;
using Travel.Web.Services.TourServices;
using Travel.Web.Validations;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using Travel.Web.Services.FavoriteServices;
using Travel.Web.Helpers;

namespace Travel.Web.Controllers
{
    public class TourController : Controller
    {
        private readonly ITourService _tourService;
        private readonly IDestinationService _destinationService;
        private readonly ICategoryService _categoryService;
        private readonly ICommentService _commentService;
        private readonly IQuestionService _questionService;
        private readonly IReservationService _reservationService;

        private readonly IValidator<CreateReservationDto> _reservationValidator;
        private readonly IValidator<CreateCommentDto> _commentValidator;
        private readonly IValidator<CreateQuestionDto> _questionValidator;

        private readonly IStringLocalizer<SharedResource> _L;
        private readonly IFavoriteService _favoriteService;

        public TourController(
            ITourService tourService,
            IDestinationService destinationService,
            ICategoryService categoryService,
            ICommentService commentService,
            IQuestionService questionService,
            IReservationService reservationService,

            IValidator<CreateReservationDto> reservationValidator,
            IValidator<CreateCommentDto> commentValidator,
            IValidator<CreateQuestionDto> questionValidator,
            IStringLocalizer<SharedResource> localizer,
            IFavoriteService favoriteService)
        {
            _tourService = tourService;
            _destinationService = destinationService;
            _categoryService = categoryService;
            _commentService = commentService;
            _questionService = questionService;
            _reservationService = reservationService;

            _reservationValidator = reservationValidator;
            _commentValidator = commentValidator;
            _questionValidator = questionValidator;

            _L = localizer;
            _favoriteService = favoriteService;
        }

        public async Task<IActionResult> Index(string? search, string? destinationId, string? categoryId, decimal? minPrice, decimal? maxPrice, string? sort)
        {
            var tours = await _tourService.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var query = search.Trim().ToLower();
                tours = tours.Where(x =>
                    (x.Title != null && x.Title.ToString().ToLower().Contains(query)) ||
                    (x.Country != null && x.Country.ToLower().Contains(query)) ||
                    (x.City != null && x.City.ToLower().Contains(query))
                ).ToList();
            }

            if (!string.IsNullOrEmpty(destinationId))
                tours = tours.Where(x => x.DestinationId == destinationId).ToList();

            if (!string.IsNullOrEmpty(categoryId))
                tours = tours.Where(x => x.CategoryId == categoryId).ToList();

            if (minPrice.HasValue)
                tours = tours.Where(x => x.Price >= minPrice.Value).ToList();

            if (maxPrice.HasValue)
                tours = tours.Where(x => x.Price <= maxPrice.Value).ToList();

            tours = sort switch
            {
                "price-asc" => tours.OrderBy(x => x.Price).ToList(),
                "price-desc" => tours.OrderByDescending(x => x.Price).ToList(),
                "days-desc" => tours.OrderByDescending(x => x.DurationDays).ToList(),
                _ => tours.OrderByDescending(x => x.IsPopular).ToList()
            };

            ViewBag.Destinations = await _destinationService.GetAllAsync();
            ViewBag.Categories = await _categoryService.GetAllAsync();

            // Kartlardaki puan ve yorum sayısı (gerçek yorumlardan, tek aggregation sorgusu)
            ViewBag.TourRatings = await _commentService.GetRatingsForToursAsync(tours.Select(x => x.Id));
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sort;

            // Kartlardaki kalp ikonları için kullanıcının favori tur Id'leri
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.FavoriteIds = string.IsNullOrEmpty(userId)
                ? new HashSet<string>()
                : (await _favoriteService.GetTourIdsByUserAsync(userId)).ToHashSet();

            return View(tours);
        }

        public async Task<IActionResult> Detail(string id)
        {
            var tour = await _tourService.GetByIdAsync(id);
            if (tour == null)
            {
                return NotFound();
            }

            // Tura ait onaylanmış yorumlar ve sorular
            var comments = await _commentService.GetCommentsByTourIdAsync(id);
            var questions = await _questionService.GetAnsweredQuestionsByTourIdAsync(id);

            // Benzer turlar (Aynı kategorideki diğer turlar)
            var allTours = await _tourService.GetAllAsync();
            var similarTours = allTours
                .Where(x => x.Id != id && x.CategoryId == tour.CategoryId)
                .Take(3)
                .ToList();

            ViewBag.Comments = comments;
            ViewBag.Questions = questions;
            ViewBag.SimilarTours = similarTours;

            // Puan özeti (ortalama + yıldız yüzdeleri) ve benzer turların puanları
            ViewBag.RatingSummary = await _commentService.GetTourRatingSummaryAsync(id);
            ViewBag.SimilarRatings = await _commentService.GetRatingsForToursAsync(similarTours.Select(x => x.Id));

            // "Kaydet" butonunun başlangıç durumu
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.IsFavorite = !string.IsNullOrEmpty(userId) && await _favoriteService.IsFavoriteAsync(userId, id);

            return View(tour);
        }

        // Case Madde 9: Favorilere ekle / çıkar (AJAX)
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(string tourId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, requiresLogin = true, message = _L["Favorilere eklemek için lütfen giriş yapın."].Value });
            }

            if (string.IsNullOrWhiteSpace(tourId) || await _tourService.GetByIdAsync(tourId) == null)
            {
                return Json(new { success = false, message = _L["Geçersiz istek."].Value });
            }

            var isFavorite = await _favoriteService.ToggleAsync(userId, tourId);

            return Json(new
            {
                success = true,
                isFavorite,
                message = isFavorite ? _L["Tur favorilere eklendi."].Value : _L["Tur favorilerden çıkarıldı."].Value
            });
        }

        [HttpPost]
        public async Task<IActionResult> BookTour([FromBody] CreateReservationDto dto)
        {
            if (dto == null)
            {
                return Json(new { success = false, message = _L["Geçersiz rezervasyon bilgisi."].Value });
            }

            // YENİ: FluentValidation kontrolü
            var validation = await _reservationValidator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                return Json(new { success = false, message = validation.ToMessage() });
            }

            // Kullanıcı bilgileri artık Session'dan değil, giriş cookie'sindeki claim'lerden okunuyor
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, requiresLogin = true, message = _L["Rezervasyon yapabilmek için lütfen önce giriş yapın."].Value });
            }

            dto.UserId = userId; // Artık e-posta değil, gerçek kullanıcı Id'si
            dto.Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            dto.FullName = User.Identity?.Name ?? dto.FullName;

            if (string.IsNullOrWhiteSpace(dto.Phone))
            {
                dto.Phone = User.FindFirstValue(ClaimTypes.MobilePhone) ?? string.Empty;
            }

            var result = await _reservationService.CreateReservationAsync(dto);
            if (!result)
            {
                return Json(new { success = false, message = _L["Seçilen tarih için yeterli kontenjan yok veya tarih geçersiz."].Value });
            }

            return Json(new { success = true, message = _L["Rezervasyonunuz başarıyla oluşturuldu! Profilinize yönlendiriliyorsunuz..."].Value });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CreateCommentDto dto)
        {
            // YENİ: Eski "Content boş mu" kontrolü yerine null kontrolü + validator
            if (dto == null)
            {
                return Json(new { success = false, message = _L["Geçersiz istek."].Value });
            }

            var validation = await _commentValidator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                return Json(new { success = false, message = validation.ToMessage() });
            }

            // Yorum yapan kullanıcı Id + ad ile kaydedilir (profilde Id ile listelenir)
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, requiresLogin = true, message = _L["Yorum yapabilmek için lütfen giriş yapın."].Value });
            }

            dto.UserId = userId;
            dto.UserName = User.Identity?.Name ?? string.Empty;

            await _commentService.CreateAsync(dto);

            return Json(new { success = true, message = _L["Yorumunuz başarıyla paylaşıldı!"].Value });
        }

        [HttpPost]
        public async Task<IActionResult> AskQuestion([FromBody] CreateQuestionDto dto)
        {
            // YENİ
            if (dto == null)
            {
                return Json(new { success = false, message = _L["Geçersiz istek."].Value });
            }

            var validation = await _questionValidator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                return Json(new { success = false, message = validation.ToMessage() });
            }

            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, requiresLogin = true, message = _L["Soru sorabilmek için lütfen giriş yapın."].Value });
            }

            dto.UserId = userId;
            dto.UserName = User.Identity?.Name ?? string.Empty;

            await _questionService.CreateAsync(dto);

            return Json(new { success = true, message = _L["Sorunuz rehbere iletildi! Yanıtlandığında bu alanda görüntülenecektir."].Value });
        }
    }
}