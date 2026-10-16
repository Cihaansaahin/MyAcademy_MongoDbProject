using Travel.Web.DTOs.CommentDtos;

namespace Travel.Web.Services.CommentServices
{
    public interface ICommentService
    {
        Task<List<ResultCommentDto>> GetAllAsync();
        Task<List<ResultCommentDto>> GetCommentsByTourIdAsync(string tourId);
        Task<List<ResultCommentDto>> GetCommentsByUserIdAsync(string userId);
        Task CreateAsync(CreateCommentDto createCommentDto);
        Task DeleteAsync(string id);
        Task<bool> ApproveCommentAsync(string id);
        Task<TourRatingSummaryDto> GetTourRatingSummaryAsync(string tourId);

        // Birden fazla tur için ortalama puan + yorum sayısı (Aggregation, tek sorgu)
        Task<Dictionary<string, TourRatingSummaryDto>> GetRatingsForToursAsync(IEnumerable<string> tourIds);

        // Kullanıcı sadece KENDİ yorumunu güncelleyebilir / silebilir
        Task<bool> UpdateRatingAsync(string commentId, string userId, int rating);
        Task<bool> DeleteByUserAsync(string commentId, string userId);
    }
}