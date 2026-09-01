namespace Travel.Web.DTOs.DestinationDtos
{
    public class ResultDestinationDto
    {
        public string Id { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPopular { get; set; }
    }
}