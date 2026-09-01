using AutoMapper;
using MongoDB.Driver;
using Travel.Web.DTOs.QuestionDtos;
using Travel.Web.Entities;
using Travel.Web.Settings;

namespace Travel.Web.Services.QuestionServices
{
    public class QuestionService : IQuestionService
    {
        private readonly IMongoCollection<Question> _questionCollection;
        private readonly IMapper _mapper;

        public QuestionService(IDataBaseSettings databaseSettings, IMapper mapper)
        {
            var client = new MongoClient(databaseSettings.ConnectionString);
            var database = client.GetDatabase(databaseSettings.DataBaseName);
            _questionCollection = database.GetCollection<Question>(databaseSettings.QuestionCollectionName);
            _mapper = mapper;
        }

        public async Task<List<ResultQuestionDto>> GetAllAsync()
        {
            var questions = await _questionCollection.Find(_ => true).ToListAsync();
            return _mapper.Map<List<ResultQuestionDto>>(questions);
        }

        // Case Madde 10: Cevaplanmamış sorular admin panelinde listelenir
        public async Task<List<ResultQuestionDto>> GetUnansweredQuestionsAsync()
        {
            var filter = Builders<Question>.Filter.Or(
                Builders<Question>.Filter.Eq(x => x.AnswerText, null),
                Builders<Question>.Filter.Eq(x => x.AnswerText, string.Empty)
            );
            var questions = await _questionCollection.Find(filter).ToListAsync();
            return _mapper.Map<List<ResultQuestionDto>>(questions);
        }

        // Case Madde 5 & 10: Cevaplanan sorular tur detayında gösterilir[cite: 1]
        public async Task<List<ResultQuestionDto>> GetAnsweredQuestionsByTourIdAsync(string tourId)
        {
            var filter = Builders<Question>.Filter.And(
                Builders<Question>.Filter.Eq(x => x.TourId, tourId),
                Builders<Question>.Filter.Ne(x => x.AnswerText, null),
                Builders<Question>.Filter.Ne(x => x.AnswerText, string.Empty)
            );
            var questions = await _questionCollection.Find(filter).ToListAsync();
            return _mapper.Map<List<ResultQuestionDto>>(questions);
        }

        public async Task<List<ResultQuestionDto>> GetQuestionsByUserIdAsync(string userId)
        {
            var questions = await _questionCollection.Find(x => x.UserId == userId).ToListAsync();
            return _mapper.Map<List<ResultQuestionDto>>(questions);
        }

        public async Task<ResultQuestionDto> GetByIdAsync(string id)
        {
            var question = await _questionCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
            return _mapper.Map<ResultQuestionDto>(question);
        }

        public async Task CreateAsync(CreateQuestionDto createQuestionDto)
        {
            var question = _mapper.Map<Question>(createQuestionDto);
            question.AskedDate = DateTime.UtcNow;
            await _questionCollection.InsertOneAsync(question);
        }

        public async Task<bool> AnswerQuestionAsync(AnswerQuestionDto answerQuestionDto)
        {
            var update = Builders<Question>.Update
                .Set(x => x.AnswerText, answerQuestionDto.AnswerText)
                .Set(x => x.AnsweredDate, DateTime.UtcNow);

            var result = await _questionCollection.UpdateOneAsync(x => x.Id == answerQuestionDto.Id, update);
            return result.ModifiedCount > 0;
        }

        public async Task DeleteAsync(string id)
        {
            await _questionCollection.DeleteOneAsync(x => x.Id == id);
        }
    }
}