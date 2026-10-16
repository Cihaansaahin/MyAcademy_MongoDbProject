using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Travel.Web.DTOs.ReservationDtos;
using Travel.Web.Services.ReportServices;
using Travel.Web.Services.ReservationServices;
using Travel.Web.Services.TourServices;

namespace Travel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Sadece rolü 'Admin' olan oturumlar girebilir
    public class ReservationController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly ITourService _tourService;
        private readonly IReportService _reportService;
        private readonly IStringLocalizer<SharedResource> _L;

        public ReservationController(
            IReservationService reservationService,
            ITourService tourService,
            IReportService reportService,
            IStringLocalizer<SharedResource> localizer)
        {
            _reservationService = reservationService;
            _tourService = tourService;
            _reportService = reportService;
            _L = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var values = await _reservationService.GetAllAsync();

            // Filtre çubuğundaki "Tur" listesi ve Rapor Al panelindeki tur/tarih seçimi için
            var tours = await _tourService.GetAllAsync();
            ViewBag.Tours = tours.OrderBy(x => x.Title?.Value).ToList();

            return View(values.OrderByDescending(x => x.ReservationDate).ToList());
        }

        public async Task<IActionResult> Approve(string id)
        {
            await _reservationService.ApproveReservationAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Cancel(string id)
        {
            await _reservationService.CancelReservationAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Excel ve PDF için ortak veri hazırlığı (tur + isteğe bağlı tur tarihi filtresi)
        private async Task<(List<ResultReservationDto> Rows, string Title, string Subtitle)?> LoadReportAsync(string tourId, string? tourDateId)
        {
            var tour = await _tourService.GetByIdAsync(tourId);
            if (tour == null) return null;

            var rows = await _reservationService.GetReservationsByTourIdAsync(tourId, tourDateId);
            var title = tour.Title?.Value ?? _L["Tur"].Value;

            var subtitle = _L["Tüm tarihler"].Value;
            if (!string.IsNullOrEmpty(tourDateId))
            {
                var date = tour.TourDates.FirstOrDefault(x => x.Id == tourDateId);
                if (date != null)
                    subtitle = $"{_L["Tur Tarihi"].Value}: {date.StartDate:dd.MM.yyyy} - {date.EndDate:dd.MM.yyyy}";
            }

            return (rows, title, subtitle);
        }

        private static string SafeFileName(string text)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var clean = new string(text.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Replace(' ', '_');
            return clean.Length > 60 ? clean[..60] : clean;
        }

        [HttpGet]
        public async Task<IActionResult> ExportTourParticipantsToExcel(string tourId, string? tourDateId = null)
        {
            if (string.IsNullOrEmpty(tourId)) return BadRequest(_L["Geçersiz istek."].Value);

            var data = await LoadReportAsync(tourId, tourDateId);
            if (data == null) return NotFound();

            var bytes = _reportService.GenerateExcelTourReport(data.Value.Rows, data.Value.Title, data.Value.Subtitle);
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{SafeFileName(data.Value.Title)}_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportTourParticipantsToPdf(string tourId, string? tourDateId = null)
        {
            if (string.IsNullOrEmpty(tourId)) return BadRequest(_L["Geçersiz istek."].Value);

            var data = await LoadReportAsync(tourId, tourDateId);
            if (data == null) return NotFound();

            var bytes = _reportService.GeneratePdfTourReport(data.Value.Rows, data.Value.Title, data.Value.Subtitle);
            return File(bytes, "application/pdf", $"{SafeFileName(data.Value.Title)}_{DateTime.Now:yyyyMMdd}.pdf");
        }
    }
}