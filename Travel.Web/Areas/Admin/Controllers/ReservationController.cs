using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using Travel.Web.Services.ReservationServices;
using Travel.Web.Services.TourServices;

namespace Travel.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReservationController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly ITourService _tourService;

        public ReservationController(IReservationService reservationService, ITourService tourService)
        {
            _reservationService = reservationService;
            _tourService = tourService;
        }

        public async Task<IActionResult> Index()
        {
            var values = await _reservationService.GetAllAsync();
            return View(values);
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

        [HttpGet]
        public async Task<IActionResult> ExportTourParticipantsToExcel(string tourId, string? tourDateId = null)
        {
            if (string.IsNullOrEmpty(tourId)) return BadRequest("Geçersiz Tur ID.");

            var reservations = await _reservationService.GetReservationsByTourIdAsync(tourId, tourDateId);
            var tour = await _tourService.GetByIdAsync(tourId);
            var tourTitle = tour?.Title?.Tr ?? tour?.Title?.ToString() ?? "Tur";

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Katılımcılar");

                worksheet.Cell(1, 1).Value = $"{tourTitle} - Katılımcı Listesi";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Range(1, 1, 1, 6).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(3, 1).Value = "Ad Soyad";
                worksheet.Cell(3, 2).Value = "E-Posta";
                worksheet.Cell(3, 3).Value = "Telefon";
                worksheet.Cell(3, 4).Value = "Kişi Sayısı";
                worksheet.Cell(3, 5).Value = "Toplam Tutar";
                worksheet.Cell(3, 6).Value = "Durum";
                worksheet.Range(3, 1, 3, 6).Style.Font.Bold = true;

                int row = 4;
                foreach (var item in reservations)
                {
                    worksheet.Cell(row, 1).Value = item.FullName;
                    worksheet.Cell(row, 2).Value = item.Email;
                    worksheet.Cell(row, 3).Value = item.Phone;
                    worksheet.Cell(row, 4).Value = item.TotalParticipants;
                    worksheet.Cell(row, 5).Value = item.TotalPrice;
                    worksheet.Cell(row, 6).Value = item.Status.ToString();
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{tourTitle}_Katilimcilar.xlsx");
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportTourParticipantsToPdf(string tourId, string? tourDateId = null)
        {
            if (string.IsNullOrEmpty(tourId)) return BadRequest("Geçersiz Tur ID.");

            var reservations = await _reservationService.GetReservationsByTourIdAsync(tourId, tourDateId);
            var tour = await _tourService.GetByIdAsync(tourId);
            var tourTitle = tour?.Title?.Tr ?? tour?.Title?.ToString() ?? "Tur";

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text($"{tourTitle} - Katılımcı Raporu").SemiBold().FontSize(18).FontColor("#0FA3A3");

                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); // Ad Soyad
                            columns.RelativeColumn(2); // E-Posta
                            columns.RelativeColumn(1.5f); // Telefon
                            columns.RelativeColumn(1); // Kişi
                            columns.RelativeColumn(1.5f); // Tutar
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Ad Soyad").Bold();
                            header.Cell().Text("E-Posta").Bold();
                            header.Cell().Text("Telefon").Bold();
                            header.Cell().Text("Kişi").Bold();
                            header.Cell().Text("Tutar").Bold();
                        });

                        foreach (var item in reservations)
                        {
                            table.Cell().Text(item.FullName ?? "-");
                            table.Cell().Text(item.Email ?? "-");
                            table.Cell().Text(item.Phone ?? "-");
                            table.Cell().Text(item.TotalParticipants.ToString());
                            table.Cell().Text($"{item.TotalPrice:N2} TL");
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Oluşturulma Tarihi: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"{tourTitle}_Katilimcilar.pdf");
        }
    }

}