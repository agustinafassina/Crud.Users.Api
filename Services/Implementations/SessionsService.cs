
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using UsersApi.Configurations.ClientsDB.Mongo;
using UsersApi.Services.Interfaces;

namespace UsersApi.Services.Implementations
{
    public class SessionsService : ISessionsService
    {
        private readonly IMongoCollection<BsonDocument> _sessionsCollection;

        public SessionsService(IMongoClient client, IOptions<MongoSettings> settings)
        {
            var database = client.GetDatabase(settings.Value.DatabaseName);
            Console.WriteLine("MongoDB connected to database: " + settings.Value.DatabaseName);
            _sessionsCollection = database.GetCollection<BsonDocument>(settings.Value.CollectionSessions);
        }

        public async Task CreateSessionAsync(int userId, System.DateTime expirationToken, string token)
        {
            var doc = new BsonDocument
            {
                { "userId", userId },
                { "created", DateTime.UtcNow },
                { "expirationToken", expirationToken },
                { "token", token }
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