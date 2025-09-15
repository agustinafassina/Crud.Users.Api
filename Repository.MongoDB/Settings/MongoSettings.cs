namespace UsersApi.Repository.MongoClient.Settings
{
    public class MongoSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionSessions { get; set; }
    }
}