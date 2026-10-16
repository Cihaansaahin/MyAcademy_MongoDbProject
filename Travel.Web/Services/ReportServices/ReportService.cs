using ClosedXML.Excel;
using Microsoft.Extensions.Localization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Travel.Web.DTOs.ReservationDtos;
using Travel.Web.Entities;

namespace Travel.Web.Services.ReportServices
{
    // Case Madde 15: Tur Katılımcı Raporu (ClosedXML + QuestPDF, MemoryStream ile — sunucuda dosya tutulmaz)
    public class ReportService : IReportService
    {
        private readonly IStringLocalizer<SharedResource> _L;

        public ReportService(IStringLocalizer<SharedResource> localizer)
        {
            _L = localizer;
        }

        // Rapor alanları (Case): Ad Soyad, E-posta, Telefon, Tur, Tur Tarihi, Yetişkin, Çocuk,
        // Toplam Kişi, Rezervasyon Tarihi, Toplam Ücret, Durum
        private string[] Headers => new[]
        {
            _L["Ad Soyad"].Value, _L["E-posta"].Value, _L["Telefon"].Value, _L["Tur"].Value, _L["Tur Tarihi"].Value,
            _L["Yetişkin"].Value, _L["Çocuk"].Value, _L["Toplam Kişi"].Value, _L["Rezervasyon Tarihi"].Value,
            _L["Toplam Ücret"].Value, _L["Durum"].Value
        };

        private string StatusText(ReservationStatus status) => status switch
        {
            ReservationStatus.Approved => _L["Onaylandı"].Value,
            ReservationStatus.Cancelled => _L["İptal Edildi"].Value,
            _ => _L["Beklemede"].Value
        };

        private static List<ResultReservationDto> Sort(List<ResultReservationDto> list) =>
            list.OrderBy(x => x.SelectedTourDate).ThenBy(x => x.FullName).ToList();

        // ---------------- EXCEL ----------------
        public byte[] GenerateExcelTourReport(List<ResultReservationDto> reservations, string tourTitle, string subtitle)
        {
            var rows = Sort(reservations);
            var headers = Headers;

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add(_L["Katılımcılar"].Value);

            // Başlık
            ws.Cell(1, 1).Value = $"{tourTitle} - {_L["Katılımcı Raporu"].Value}";
            ws.Range(1, 1, 1, headers.Length).Merge();
            ws.Cell(1, 1).Style.Font.SetBold().Font.SetFontSize(14).Font.SetFontColor(XLColor.FromHtml("#0FA3A3"));

            ws.Cell(2, 1).Value = $"{subtitle}  ·  {_L["Oluşturulma Tarihi"].Value}: {DateTime.Now:dd.MM.yyyy HH:mm}";
            ws.Range(2, 1, 2, headers.Length).Merge();
            ws.Cell(2, 1).Style.Font.SetFontColor(XLColor.Gray);

            // Tablo başlıkları
            const int headerRow = 4;
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(headerRow, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#0E2A3B");
            }

            // Satırlar
            int row = headerRow + 1;
            foreach (var item in rows)
            {
                ws.Cell(row, 1).Value = item.FullName;
                ws.Cell(row, 2).Value = item.Email;
                ws.Cell(row, 3).Value = item.Phone;
                ws.Cell(row, 4).Value = item.TourTitle;
                ws.Cell(row, 5).Value = item.SelectedTourDate;
                ws.Cell(row, 5).Style.DateFormat.Format = "dd.MM.yyyy";
                ws.Cell(row, 6).Value = item.AdultCount;
                ws.Cell(row, 7).Value = item.ChildCount;
                ws.Cell(row, 8).Value = item.TotalParticipants;
                ws.Cell(row, 9).Value = item.ReservationDate.ToLocalTime();
                ws.Cell(row, 9).Style.DateFormat.Format = "dd.MM.yyyy HH:mm";
                ws.Cell(row, 10).Value = item.TotalPrice;
                ws.Cell(row, 10).Style.NumberFormat.Format = "#,##0.00 \"TL\"";
                ws.Cell(row, 11).Value = StatusText(item.Status);

                if (item.Status == ReservationStatus.Cancelled)
                    ws.Range(row, 1, row, headers.Length).Style.Font.FontColor = XLColor.Gray;

                row++;
            }

            // Toplam satırı (iptaller hariç)
            var active = rows.Where(x => x.Status != ReservationStatus.Cancelled).ToList();
            ws.Cell(row, 1).Value = _L["Toplam (iptaller hariç)"].Value;
            ws.Cell(row, 6).Value = active.Sum(x => x.AdultCount);
            ws.Cell(row, 7).Value = active.Sum(x => x.ChildCount);
            ws.Cell(row, 8).Value = active.Sum(x => x.TotalParticipants);
            ws.Cell(row, 10).Value = active.Sum(x => x.TotalPrice);
            ws.Cell(row, 10).Style.NumberFormat.Format = "#,##0.00 \"TL\"";
            ws.Range(row, 1, row, headers.Length).Style.Font.Bold = true;
            ws.Range(row, 1, row, headers.Length).Style.Border.TopBorder = XLBorderStyleValues.Thin;

            ws.SheetView.FreezeRows(headerRow);
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // ---------------- PDF ----------------
        public byte[] GeneratePdfTourReport(List<ResultReservationDto> reservations, string tourTitle, string subtitle)
        {
            var rows = Sort(reservations);
            var headers = Headers;
            var active = rows.Where(x => x.Status != ReservationStatus.Cancelled).ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape()); // 11 sütun için yatay sayfa
                    page.Margin(25);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    page.Header().PaddingBottom(8).Column(col =>
                    {
                        col.Item().Text($"{tourTitle} - {_L["Katılımcı Raporu"].Value}").SemiBold().FontSize(16).FontColor("#0FA3A3");
                        col.Item().Text(subtitle).FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1.6f); // Ad Soyad
                            c.RelativeColumn(2f);   // E-posta
                            c.RelativeColumn(1.3f); // Telefon
                            c.RelativeColumn(1.8f); // Tur
                            c.RelativeColumn(1f);   // Tur Tarihi
                            c.RelativeColumn(0.7f); // Yetişkin
                            c.RelativeColumn(0.6f); // Çocuk
                            c.RelativeColumn(0.7f); // Toplam Kişi
                            c.RelativeColumn(1.2f); // Rezervasyon Tarihi
                            c.RelativeColumn(1.1f); // Toplam Ücret
                            c.RelativeColumn(0.9f); // Durum
                        });

                        table.Header(header =>
                        {
                            foreach (var h in headers)
                                header.Cell().Background("#0E2A3B").Padding(4).Text(h).FontColor(Colors.White).SemiBold();
                        });

                        var i = 0;
                        foreach (var item in rows)
                        {
                            Color bg = i++ % 2 == 0 ? Colors.White : Color.FromHex("#F3F7F7");
                            var color = item.Status == ReservationStatus.Cancelled ? Colors.Grey.Medium : Colors.Black;

                            void Cell(string text) =>
                                table.Cell().Background(bg).Padding(4).Text(text).FontColor(color);

                            Cell(item.FullName ?? "-");
                            Cell(item.Email ?? "-");
                            Cell(item.Phone ?? "-");
                            Cell(item.TourTitle ?? "-");
                            Cell(item.SelectedTourDate.ToString("dd.MM.yyyy"));
                            Cell(item.AdultCount.ToString());
                            Cell(item.ChildCount.ToString());
                            Cell(item.TotalParticipants.ToString());
                            Cell(item.ReservationDate.ToLocalTime().ToString("dd.MM.yyyy HH:mm"));
                            Cell($"{item.TotalPrice:N2} TL");
                            Cell(StatusText(item.Status));
                        }

                        // Toplam satırı
                        table.Cell().ColumnSpan(5).BorderTop(1).Padding(4).Text(_L["Toplam (iptaller hariç)"].Value).SemiBold();
                        table.Cell().BorderTop(1).Padding(4).Text(active.Sum(x => x.AdultCount).ToString()).SemiBold();
                        table.Cell().BorderTop(1).Padding(4).Text(active.Sum(x => x.ChildCount).ToString()).SemiBold();
                        table.Cell().BorderTop(1).Padding(4).Text(active.Sum(x => x.TotalParticipants).ToString()).SemiBold();
                        table.Cell().BorderTop(1).Padding(4).Text("");
                        table.Cell().BorderTop(1).Padding(4).Text($"{active.Sum(x => x.TotalPrice):N2} TL").SemiBold();
                        table.Cell().BorderTop(1).Padding(4).Text("");
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span($"{_L["Oluşturulma Tarihi"].Value}: {DateTime.Now:dd.MM.yyyy HH:mm}  ·  ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf(); // byte[] — doğrudan bellekte üretilir
        }
    }
}