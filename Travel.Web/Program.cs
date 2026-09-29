using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Reflection;
using Travel.Web.Services.BannerServices;
using Travel.Web.Services.CategoryServices;
using Travel.Web.Services.CommentServices;
using Travel.Web.Services.DashboardServices;
using Travel.Web.Services.DestinationServices;
using Travel.Web.Services.QuestionServices;
using Travel.Web.Services.ReportServices;
using Travel.Web.Services.ReservationServices;
using Travel.Web.Services.TourServices;
using Travel.Web.Settings;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// 1. SERVİS VE BAĞIMLILIK KAYITLARI (BUILD ÖNCESİ)
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.Configure<DataBaseSettings>(builder.Configuration.GetSection(nameof(DataBaseSettings)));

builder.Services.AddScoped<IBannerService, BannerService>();
builder.Services.AddScoped<ITourService, TourService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IDestinationService, DestinationService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();

builder.Services.AddSingleton<IDataBaseSettings>(sp =>
{
    return sp.GetRequiredService<IOptions<DataBaseSettings>>().Value;
});

// Oturum (Session) servisi
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

// --- KİMLİK DOĞRULAMA (AUTHENTICATION) & COOKIE AYARLARI ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Travelio.Auth";
        options.LoginPath = "/Account/Login";              // Giriş yapmamışsa buraya yönlendirir
        options.AccessDeniedPath = "/Account/AccessDenied"; // Yetkisi yetersizse (Admin değilse) buraya yönlendirir
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

// Case Madde 12: Localization Servisleri
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

// ==========================================
// 2. UYGULAMANIN DERLENMESİ
// ==========================================
var app = builder.Build();

// 3. HTTP PIPELINE & MIDDLEWARE YAPILANDIRMASI
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Request Localization Middleware
var supportedCultures = new[] { "tr-TR", "en-US" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("tr-TR")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

localizationOptions.RequestCultureProviders = new List<IRequestCultureProvider>
{
    new CookieRequestCultureProvider()
};
app.UseRequestLocalization(localizationOptions);

// Oturum
app.UseSession();

// SIRALAMA ÇOK ÖNEMLİ:
app.UseAuthentication(); // 1. Kimsin? (Giriş yapılmış mı?)
app.UseAuthorization();  // 2. Yetkin var mı? (Admin misin?)

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();