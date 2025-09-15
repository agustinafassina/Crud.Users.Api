
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
        private readonly List<ItemDto> _items = new();

        public SessionsService(IMongoClient client, IOptions<MongoSettings> settings)
        {
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _sessionsCollection = database.GetCollection<BsonDocument>(settings.Value.CollectionSessions);
        }

        public IEnumerable<ItemDto> GetAllItems()
        {
            return _items;
        }

        public ItemDto GetItemById(int id)
        {
            return _items.FirstOrDefault(i => i.Id == id);
        }

        public ItemDto CreateItem(ItemCreateDto newItem)
        {
            int newId = _items.Max(i => i.Id) + 1;
            ItemDto item = new ItemDto { Id = newId, Name = newItem.Name };
            _items.Add(item);
            return item;
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