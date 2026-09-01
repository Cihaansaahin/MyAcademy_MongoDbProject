namespace Travel.Web.DTOs.QuestionDtos
{
    public class CreateQuestionDto
    {
        public string TourId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
    }
}