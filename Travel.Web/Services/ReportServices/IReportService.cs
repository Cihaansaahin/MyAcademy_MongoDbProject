using Travel.Web.DTOs.ReservationDtos;

namespace Travel.Web.Services.ReportServices
{
    public interface IReportService
    {
        // subtitle: "Tur Tarihi: 01.11.2026 - 05.11.2026" veya "Tüm tarihler"
        byte[] GenerateExcelTourReport(List<ResultReservationDto> reservations, string tourTitle, string subtitle);
        byte[] GeneratePdfTourReport(List<ResultReservationDto> reservations, string tourTitle, string subtitle);
    }
}