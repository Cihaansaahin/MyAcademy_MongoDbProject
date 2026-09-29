using Microsoft.AspNetCore.Mvc;
using Travel.Web.DTOs.IdentityDtos;

namespace Travel.Web.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(CreateRegisterDto createRegisterDto)
        {
            if (!ModelState.IsValid)
            {
                return View(createRegisterDto);
            }

            // Bilgileri Session'a kaydediyoruz:
            HttpContext.Session.SetString("UserName", $"{createRegisterDto.FirstName} {createRegisterDto.LastName}".Trim());
            HttpContext.Session.SetString("UserEmail", createRegisterDto.Email);
            HttpContext.Session.SetString("UserPhone", createRegisterDto.PhoneNumber ?? "");

            TempData["SuccessMessage"] = $"Hoş geldin {createRegisterDto.FirstName}!";
            return RedirectToAction("Index", "Profile");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return View(loginDto);
            }

            // Admin kontrolü
            if (loginDto.Email == "admin@travelio.com" && loginDto.Password == "Admin123*")
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            // Giriş yapan kullanıcının bilgilerini Session'a kaydediyoruz:
            var name = loginDto.Email.Split('@')[0];
            var formattedName = char.ToUpper(name[0]) + name.Substring(1);

            HttpContext.Session.SetString("UserName", formattedName);
            HttpContext.Session.SetString("UserEmail", loginDto.Email);

            TempData["LoginUser"] = loginDto.Email;
            return RedirectToAction("Index", "Profile");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}