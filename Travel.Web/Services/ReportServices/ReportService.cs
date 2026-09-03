using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Travel.Web.DTOs.ReservationDtos;

namespace Travel.Web.Services.ReportServices
{
    public class ReportService : IReportService
    {
        public ReportService()
        {
            // QuestPDF Community Lisans Ayarı (Ücretsiz kullanım için zorunludur)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // Case Madde 15: Excel Katılımcı Raporu (ClosedXML - MemoryStream)
        public byte[] GenerateExcelTourReport(List<ResultReservationDto> reservations, string tourTitle)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Katılımcı Listesi");

            // Başlık Alanları
            string[] headers = {
                "Ad Soyad", "E-posta", "Telefon", "Tur Adı", "Tur Tarihi",
                "Yetişkin", "Çocuk", "Toplam Kişi", "Rezervasyon Tarihi", "Toplam Ücret", "Durum"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(41, 128, 185);
                cell.Style.Font.FontColor = XLColor.White;
            }

            // Satırları doldurma
            int row = 2;
            foreach (var item in reservations)
            {
                worksheet.Cell(row, 1).Value = item.FullName;
                worksheet.Cell(row, 2).Value = item.Email;
                worksheet.Cell(row, 3).Value = item.Phone;
                worksheet.Cell(row, 4).Value = item.TourTitle;
                worksheet.Cell(row, 5).Value = item.SelectedTourDate.ToString("dd.MM.yyyy");
                worksheet.Cell(row, 6).Value = item.AdultCount;
                worksheet.Cell(row, 7).Value = item.ChildCount;
                worksheet.Cell(row, 8).Value = item.TotalParticipants;
                worksheet.Cell(row, 9).Value = item.ReservationDate.ToString("dd.MM.yyyy HH:mm");
                worksheet.Cell(row, 10).Value = $"{item.TotalPrice:N2} ₺";
                worksheet.Cell(row, 11).Value = item.Status.ToString();
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // Case Madde 15: PDF Katılımcı Raporu (QuestPDF - MemoryStream)
        public byte[] GeneratePdfTourReport(List<ResultReservationDto> reservations, string tourTitle)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("TRAVELIO REZERVASYON VE KATILIMCI RAPORU").Bold().FontSize(16).FontColor(Colors.Blue.Medium);
                            col.Item().Text($"Tur: {tourTitle} | Rapor Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(10).FontColor(Colors.Grey.Medium);
                        });
                    });

                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); // Ad Soyad
                            columns.RelativeColumn(2); // E-posta
                            columns.RelativeColumn(1.5f); // Telefon
                            columns.RelativeColumn(1.2f); // Tarih
                            columns.RelativeColumn(0.8f); // Yet.
                            columns.RelativeColumn(0.8f); // Çoc.
                            columns.RelativeColumn(0.8f); // Top.
                            columns.RelativeColumn(1.2f); // Ücret
                            columns.RelativeColumn(1.2f); // Durum
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Medium).Padding(4).Text("Ad Soyad").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Medium).Padding(4).Text("E-posta").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Medium).Padding(4).Text("Telefon").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Medium).Padding(4).Text("Tur Tarihi").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Medium).Padding(4).Text("Yet.").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Medium).Padding(4).Text("Çoc.").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Medium).Padding(4).Text("Top.").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Medium).Padding(4).Text("Ücret").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Medium).Padding(4).Text("Durum").FontColor(Colors.White).Bold();
                        });

                        foreach (var item in reservations)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.FullName);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.Email);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.Phone);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.SelectedTourDate.ToString("dd.MM.yyyy"));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.AdultCount.ToString());
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.ChildCount.ToString());
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.TotalParticipants.ToString());
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"{item.TotalPrice:N2} ₺");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.Status.ToString());
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }
    }
}