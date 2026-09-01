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
        public async Task<TourRatingSummaryDto> GetTourRatingSummaryAsync(string tourId)
        {
            var comments = await _commentCollection.Find(x => x.TourId == tourId && x.IsApproved).ToListAsync();

            var summary = new TourRatingSummaryDto
            {
                TotalReviews = comments.Count,
                AverageRating = comments.Any() ? Math.Round(comments.Average(x => x.Rating), 1) : 0
            };

            for (int star = 1; star <= 5; star++)
            {
                int count = comments.Count(x => x.Rating == star);
                double percentage = comments.Any() ? Math.Round(((double)count / comments.Count) * 100, 1) : 0;
                summary.StarPercentages[star] = percentage;
            }

            return summary;
        }
    }
}