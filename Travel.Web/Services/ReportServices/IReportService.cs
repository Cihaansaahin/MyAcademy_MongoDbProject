using Travel.Web.DTOs.ReservationDtos;

namespace Travel.Web.Services.ReportServices
{
    public interface IReportService
    {
        byte[] GenerateExcelTourReport(List<ResultReservationDto> reservations, string tourTitle);
        byte[] GeneratePdfTourReport(List<ResultReservationDto> reservations, string tourTitle);
    }
}