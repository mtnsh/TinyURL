namespace TinyURL
{
    using MongoDB.Driver;
    using TinyURL.Model;

    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(string connectionString, string dbName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(dbName);
        }

        public IMongoCollection<UrlMapping> UrlMappings => _database.GetCollection<UrlMapping>("urlMappings");
    }
}
