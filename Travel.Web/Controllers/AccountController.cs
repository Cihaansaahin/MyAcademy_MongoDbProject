using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using Travel.Web.DTOs.IdentityDtos;

namespace Travel.Web.Controllers
{
    public class AccountController(IStringLocalizer<SharedResource> _L) : Controller
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

            var fullName = $"{createRegisterDto.FirstName} {createRegisterDto.LastName}".Trim();

            // 1. Session kayıtları
            HttpContext.Session.SetString("UserName", fullName);
            HttpContext.Session.SetString("UserEmail", createRegisterDto.Email);
            HttpContext.Session.SetString("UserPhone", createRegisterDto.PhoneNumber ?? "");

            // 2. Cookie Authentication ile üye girişi yaptır (Rol: Member)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, fullName),
                new Claim(ClaimTypes.Email, createRegisterDto.Email),
                new Claim(ClaimTypes.Role, "Member") // Normal üye
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            TempData["SuccessMessage"] = _L["Hoş geldin {0}!", createRegisterDto.FirstName].Value;
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

            // 1. ADMIN GİRİŞİ
            if (loginDto.Email == "admin@travelio.com" && loginDto.Password == "Admin123*")
            {
                var adminClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "Admin"),
                    new Claim(ClaimTypes.Email, loginDto.Email),
                    new Claim(ClaimTypes.Role, "Admin") // ÖNEMLİ: Admin Rolü Tanımlandı
                };

                var adminIdentity = new ClaimsIdentity(adminClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(adminIdentity));

                HttpContext.Session.SetString("UserName", "Admin");
                HttpContext.Session.SetString("UserEmail", loginDto.Email);
                HttpContext.Session.SetString("UserRole", "Admin");

                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            // 2. NORMAL KULLANICI GİRİŞİ
            var name = loginDto.Email.Split('@')[0];
            var formattedName = char.ToUpper(name[0]) + name.Substring(1);

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, formattedName),
                new Claim(ClaimTypes.Email, loginDto.Email),
                new Claim(ClaimTypes.Role, "Member") // ÖNEMLİ: Standart Üye Rolü
            };

            var userIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(userIdentity));

            HttpContext.Session.SetString("UserName", formattedName);
            HttpContext.Session.SetString("UserEmail", loginDto.Email);
            HttpContext.Session.SetString("UserRole", "Member");

            TempData["LoginUser"] = loginDto.Email;
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