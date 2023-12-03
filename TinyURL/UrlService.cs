using MongoDB.Driver;
using TinyURL.Model;

namespace TinyURL
{
    public class UrlService : IUrlService
    {
        private readonly MongoDbContext _dbContext;

        public UrlService(MongoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> AddUrlMappingAsync(string longUrl, string shortUrl)
        {
            var urlMapping = new UrlMapping
            {
                LongUrl = longUrl,
                ShortUrl = shortUrl,
                AccessCount = 0,
                LastAccessed = DateTime.UtcNow
            };

            await _dbContext.UrlMappings.InsertOneAsync(urlMapping);
            return shortUrl;
        }

        public async Task<string> GetLongUrlAsync(string shortUrl)
        {
            var filter = Builders<UrlMapping>.Filter.Eq(u => u.ShortUrl, shortUrl);
            var urlMapping = await _dbContext.UrlMappings.Find(filter).FirstOrDefaultAsync();

            if (urlMapping != null)
            {
                // Optionally update the access count and last accessed time
                var update = Builders<UrlMapping>.Update
                    .Set(u => u.AccessCount, urlMapping.AccessCount + 1)
                    .Set(u => u.LastAccessed, DateTime.UtcNow);

                await _dbContext.UrlMappings.UpdateOneAsync(filter, update);

                return urlMapping.LongUrl;
            }

            return null; // Or handle as needed 
        }
    }
}
