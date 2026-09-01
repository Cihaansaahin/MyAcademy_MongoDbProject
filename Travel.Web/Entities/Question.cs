using Travel.Web.Entities.Common;

namespace Travel.Web.Entities
{
    public class Question : BaseEntity
    {
        public string TourId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public string? AnswerText { get; set; }
        public DateTime AskedDate { get; set; } = DateTime.UtcNow;
        public DateTime? AnsweredDate { get; set; }
        public bool IsAnswered => !string.IsNullOrEmpty(AnswerText);
    }
}
