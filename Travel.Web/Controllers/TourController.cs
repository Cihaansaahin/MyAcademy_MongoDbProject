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

        public TourController(
            ITourService tourService,
            IDestinationService destinationService,
            ICategoryService categoryService,
            ICommentService commentService,
            IQuestionService questionService,
            IReservationService reservationService)
        {
            _tourService = tourService;
            _destinationService = destinationService;
            _categoryService = categoryService;
            _commentService = commentService;
            _questionService = questionService;
            _reservationService = reservationService;
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
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sort;

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

            return View(tour);
        }

        [HttpPost]
        public async Task<IActionResult> BookTour([FromBody] CreateReservationDto dto)
        {
            if (dto == null)
            {
                return Json(new { success = false, message = "Geçersiz rezervasyon bilgisi." });
            }

            var sessionEmail = HttpContext.Session.GetString("UserEmail");
            var sessionName = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(sessionEmail))
            {
                return Json(new { success = false, message = "Rezervasyon yapabilmek için lütfen önce giriş yapın." });
            }

            dto.Email = sessionEmail;
            dto.FullName = !string.IsNullOrEmpty(sessionName) ? sessionName : (!string.IsNullOrEmpty(dto.FullName) ? dto.FullName : "Gezgin Misafir");
            dto.UserId = sessionEmail;

            if (string.IsNullOrEmpty(dto.Phone))
            {
                dto.Phone = "+90 555 000 00 00";
            }

            // Kontenjan kontrolü yapan ve TourDateId eşleşmesini sağlayan metot
            var result = await _reservationService.CreateReservationAsync(dto);
            if (!result)
            {
                return Json(new { success = false, message = "Seçilen tarihte yeterli kontenjan bulunmamaktadır." });
            }

            return Json(new { success = true, message = "Rezervasyonunuz başarıyla oluşturuldu! Profilinize yönlendiriliyorsunuz..." });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CreateCommentDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Content))
            {
                return Json(new { success = false, message = "Lütfen bir yorum metni yazın." });
            }

            var sessionUser = HttpContext.Session.GetString("UserName");
            if (!string.IsNullOrEmpty(sessionUser))
            {
                dto.UserName = sessionUser;
            }
            else if (string.IsNullOrEmpty(dto.UserName))
            {
                dto.UserName = "Gezgin Misafir";
            }

            await _commentService.CreateAsync(dto);

            return Json(new { success = true, message = "Yorumunuz başarıyla paylaşıldı!" });
        }

        [HttpPost]
        public async Task<IActionResult> AskQuestion([FromBody] CreateQuestionDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.QuestionText))
            {
                return Json(new { success = false, message = "Lütfen sorunuzu yazın." });
            }

            var sessionUser = HttpContext.Session.GetString("UserName");
            if (!string.IsNullOrEmpty(sessionUser))
            {
                dto.UserName = sessionUser;
            }
            else if (string.IsNullOrEmpty(dto.UserName))
            {
                dto.UserName = "Gezgin Misafir";
            }

            await _questionService.CreateAsync(dto);

            return Json(new { success = true, message = "Sorunuz rehbere iletildi! Yanıtlandığında bu alanda görüntülenecektir." });
        }
    }
}