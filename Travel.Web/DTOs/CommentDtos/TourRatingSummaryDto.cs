namespace Travel.Web.DTOs.CommentDtos
{
    public class TourRatingSummaryDto
    {
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, double> StarPercentages { get; set; } = new(); // 1-5 arası yüzdeler
    }
}