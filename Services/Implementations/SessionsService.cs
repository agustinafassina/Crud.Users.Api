
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using UsersApi.Repository.MongoClient.Settings;
using UsersApi.Services.Dto;
using UsersApi.Services.Interfaces;

namespace UsersApi.Services.Implementations
{
    public class SessionsService : ISessionsService
    {
        private readonly IMongoCollection<BsonDocument> _sessionsCollection;

        public SessionsService(IMongoClient client, IOptions<MongoSettings> settings)
        {
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _sessionsCollection = database.GetCollection<BsonDocument>(settings.Value.CollectionSessions);
        }

        public async Task CreateSessionAsync(string userId, string sessionData)
        {
            var doc = new BsonDocument
            {
                { "userId", userId },
                { "createdDate", DateTime.UtcNow },
                { "sessionData", sessionData }
            };

            await _sessionsCollection.InsertOneAsync(doc);
        }

        public async Task<BsonDocument> GetSessionByUserIdAsync(string userId)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);
            return await _sessionsCollection.Find(filter).FirstOrDefaultAsync();
        }

    }
}