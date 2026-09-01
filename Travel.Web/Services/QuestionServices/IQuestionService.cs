using Travel.Web.DTOs.QuestionDtos;

namespace Travel.Web.Services.QuestionServices
{
    public interface IQuestionService
    {
        Task<List<ResultQuestionDto>> GetAllAsync();
        Task<List<ResultQuestionDto>> GetUnansweredQuestionsAsync();
        Task<List<ResultQuestionDto>> GetAnsweredQuestionsByTourIdAsync(string tourId);
        Task<List<ResultQuestionDto>> GetQuestionsByUserIdAsync(string userId);
        Task<ResultQuestionDto> GetByIdAsync(string id);
        Task CreateAsync(CreateQuestionDto createQuestionDto);
        Task<bool> AnswerQuestionAsync(AnswerQuestionDto answerQuestionDto);
        Task DeleteAsync(string id);
    }
}