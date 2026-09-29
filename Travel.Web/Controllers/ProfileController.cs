using Microsoft.AspNetCore.Mvc;
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

        public ProfileController(
            IReservationService reservationService,
            ICommentService commentService,
            IQuestionService questionService)
        {
            _reservationService = reservationService;
            _commentService = commentService;
            _questionService = questionService;
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

            TempData["ProfileUpdateSuccess"] = "Profil bilgileriniz başarıyla güncellendi.";
            return RedirectToAction("Index");
        }
    }
}