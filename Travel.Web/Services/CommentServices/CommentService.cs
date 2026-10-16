using AutoMapper;
using MongoDB.Driver;
using Travel.Web.DTOs.CommentDtos;
using Travel.Web.Entities;
using Travel.Web.Settings;

namespace Travel.Web.Services.CommentServices
{
    public class CommentService : ICommentService
    {
        private readonly IMongoCollection<Comment> _commentCollection;
        private readonly IMapper _mapper;

        public CommentService(IDataBaseSettings databaseSettings, IMapper mapper)
        {
            var client = new MongoClient(databaseSettings.ConnectionString);
            var database = client.GetDatabase(databaseSettings.DataBaseName);
            _commentCollection = database.GetCollection<Comment>(databaseSettings.CommentCollectionName);
            _mapper = mapper;
        }

        public async Task<List<ResultCommentDto>> GetAllAsync()
        {
            var comments = await _commentCollection.Find(_ => true).ToListAsync();
            return _mapper.Map<List<ResultCommentDto>>(comments);
        }

        public async Task<List<ResultCommentDto>> GetCommentsByTourIdAsync(string tourId)
        {
            var comments = await _commentCollection.Find(x => x.TourId == tourId && x.IsApproved).ToListAsync();
            return _mapper.Map<List<ResultCommentDto>>(comments);
        }

        public async Task<List<ResultCommentDto>> GetCommentsByUserIdAsync(string userId)
        {
            var comments = await _commentCollection.Find(x => x.UserId == userId).ToListAsync();
            return _mapper.Map<List<ResultCommentDto>>(comments);
        }

        public async Task CreateAsync(CreateCommentDto createCommentDto)
        {
            var comment = _mapper.Map<Comment>(createCommentDto);
            comment.CreatedDate = DateTime.UtcNow;
            comment.IsApproved = true; // İsteğe göre varsayılan onaylı
            await _commentCollection.InsertOneAsync(comment);
        }

        public async Task DeleteAsync(string id)
        {
            await _commentCollection.DeleteOneAsync(x => x.Id == id);
        }

        public async Task<bool> ApproveCommentAsync(string id)
        {
            var update = Builders<Comment>.Update.Set(x => x.IsApproved, true);
            var result = await _commentCollection.UpdateOneAsync(x => x.Id == id, update);
            return result.ModifiedCount > 0;
        }

        // Case Madde 9: Ortalama puan ve 1-5 yıldız dağılımı hesabı
        // Case: Tur detayındaki puan özeti — MongoDB Aggregation ile
        // Onaylı yorumları puana göre gruplar: { Star: 5, Count: 12 }, { Star: 4, Count: 3 } ...
        public async Task<TourRatingSummaryDto> GetTourRatingSummaryAsync(string tourId)
        {
            var groups = await _commentCollection.Aggregate()
                .Match(x => x.TourId == tourId && x.IsApproved)
                .Group(x => x.Rating, g => new { Star = g.Key, Count = g.Count() })
                .ToListAsync();

            var total = groups.Sum(g => g.Count);

            var summary = new TourRatingSummaryDto
            {
                TotalReviews = total,
                AverageRating = total == 0 ? 0 : Math.Round(groups.Sum(g => (double)g.Star * g.Count) / total, 1)
            };

            for (int star = 1; star <= 5; star++)
            {
                var count = groups.FirstOrDefault(g => g.Star == star)?.Count ?? 0;
                summary.StarPercentages[star] = total == 0 ? 0 : Math.Round(count * 100.0 / total, 0);
            }

            return summary;
        }

        // Tur listesi / benzer turlar / favoriler için: tur başına ortalama puan ve yorum sayısı
        public async Task<Dictionary<string, TourRatingSummaryDto>> GetRatingsForToursAsync(IEnumerable<string> tourIds)
        {
            var ids = tourIds.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            if (ids.Count == 0)
                return new Dictionary<string, TourRatingSummaryDto>();

            var groups = await _commentCollection.Aggregate()
                .Match(x => ids.Contains(x.TourId) && x.IsApproved)
                .Group(x => x.TourId, g => new
                {
                    TourId = g.Key,
                    Average = g.Average(c => c.Rating),
                    Count = g.Count()
                })
                .ToListAsync();

            return groups.ToDictionary(
                g => g.TourId,
                g => new TourRatingSummaryDto
                {
                    AverageRating = Math.Round(g.Average, 1),
                    TotalReviews = g.Count
                });
        }

        public async Task<bool> UpdateRatingAsync(string commentId, string userId, int rating)
        {
            if (rating < 1 || rating > 5)
                return false;

            var result = await _commentCollection.UpdateOneAsync(
                x => x.Id == commentId && x.UserId == userId,
                Builders<Comment>.Update.Set(x => x.Rating, rating));

            return result.MatchedCount > 0;
        }

        public async Task<bool> DeleteByUserAsync(string commentId, string userId)
        {
            var result = await _commentCollection.DeleteOneAsync(x => x.Id == commentId && x.UserId == userId);
            return result.DeletedCount > 0;
        }
    }
}