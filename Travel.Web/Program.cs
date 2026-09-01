using FluentValidation;
using Microsoft.Extensions.Options;
using System.Reflection;
using Travel.Web.Services.BannerServices;
using Travel.Web.Services.CategoryServices;
using Travel.Web.Services.CommentServices;
using Travel.Web.Services.DestinationServices;
using Travel.Web.Services.ReservationServices;
using Travel.Web.Services.TourServices;
using Travel.Web.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.Configure<DataBaseSettings>(builder.Configuration.GetSection(nameof(DataBaseSettings)));

builder.Services.AddScoped<IBannerService, BannerService>();
builder.Services.AddScoped<ITourService, TourService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IDestinationService, DestinationService>();
builder.Services.AddScoped<ICommentService, CommentService>();

builder.Services.AddSingleton<IDataBaseSettings>(sp =>
{
    return sp.GetRequiredService<IOptions<DataBaseSettings>>().Value;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
