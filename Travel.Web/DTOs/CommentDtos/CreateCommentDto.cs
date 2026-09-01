namespace Travel.Web.DTOs.CommentDtos
{
    public class CreateCommentDto
    {
        public string TourId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; } // 1 - 5
    }
}