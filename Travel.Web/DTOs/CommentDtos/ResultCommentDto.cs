namespace Travel.Web.DTOs.CommentDtos
{
    public class ResultCommentDto
    {
        public string Id { get; set; } = string.Empty;
        public string TourId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsApproved { get; set; }
    }
}