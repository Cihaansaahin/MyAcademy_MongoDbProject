namespace Travel.Web.DTOs.QuestionDtos
{
    public class ResultQuestionDto
    {
        public string Id { get; set; } = string.Empty;
        public string TourId { get; set; } = string.Empty;
        public string TourTitle { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public string? AnswerText { get; set; }
        public DateTime AskedDate { get; set; }
        public DateTime? AnsweredDate { get; set; }
        public bool IsAnswered => !string.IsNullOrEmpty(AnswerText);
    }
}