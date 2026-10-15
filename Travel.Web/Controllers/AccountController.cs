using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using Travel.Web.DTOs.IdentityDtos;
using Travel.Web.Entities;
using Travel.Web.Services.UserServices;

namespace Travel.Web.Controllers
{
    public class AccountController(IUserService _userService, IStringLocalizer<SharedResource> _L) : Controller
    {
        // Cookie + Session'a kullanıcı bilgilerini yazar (Register ve Login ortak kullanır)
        private async Task SignInUserAsync(User user, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = rememberMe });

            HttpContext.Session.SetString("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserPhone", user.PhoneNumber ?? "");
            HttpContext.Session.SetString("UserRole", user.Role);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(CreateRegisterDto createRegisterDto)
        {
            if (!createRegisterDto.AgreeTerms)
            {
                ModelState.AddModelError(nameof(CreateRegisterDto.AgreeTerms),
                    _L["Devam etmek için üyelik sözleşmesini kabul etmelisiniz."].Value);
            }

            if (!ModelState.IsValid)
            {
                return View(createRegisterDto);
            }

            var user = await _userService.RegisterAsync(createRegisterDto);
            if (user == null)
            {
                ModelState.AddModelError(nameof(CreateRegisterDto.Email),
                    _L["Bu e-posta adresiyle kayıtlı bir hesap zaten var."].Value);
                return View(createRegisterDto);
            }

            await SignInUserAsync(user, rememberMe: false);

            TempData["SuccessMessage"] = _L["Hoş geldin {0}!", user.FirstName].Value;
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

            // Admin dahil herkes veritabanından doğrulanır
            var user = await _userService.ValidateCredentialsAsync(loginDto.Email, loginDto.Password);
            if (user == null)
            {
                ModelState.AddModelError(nameof(LoginDto.Password), _L["E-posta veya şifre hatalı."].Value);
                return View(loginDto);
            }

            await SignInUserAsync(user, loginDto.RememberMe);

            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            return RedirectToAction("Index", "Profile");
        }

        public async Task<IActionResult> Logout()
        {
            // Hem Session'ı hem de Tarayıcı Cookie'sini temizle
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            TempData["ErrorMessage"] = _L["Bu alana erişim yetkiniz bulunmamaktadır!"].Value;
            return RedirectToAction("Index", "Home");
        }
    }
}