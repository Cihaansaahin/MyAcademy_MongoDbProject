using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Travel.Web.Entities;

namespace Travel.Web.Helpers
{
    public static class AuthExtensions
    {
        // Cookie + Session'a kullanıcı bilgilerini yazar.
        // AccountController (Register/Login) ve ProfileController (UpdateProfile) ortak kullanır.
        public static async Task SignInUserAsync(this HttpContext http, User user, bool isPersistent)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await http.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = isPersistent });

            http.Session.SetString("UserId", user.Id);
            http.Session.SetString("UserName", user.FullName);
            http.Session.SetString("UserEmail", user.Email);
            http.Session.SetString("UserPhone", user.PhoneNumber ?? string.Empty);
            http.Session.SetString("UserRole", user.Role);
        }

        // Giriş yapmış kullanıcının MongoDB Id'si (giriş yoksa null)
        public static string? GetUserId(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}