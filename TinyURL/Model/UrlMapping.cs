using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace TinyURL.Model
{
    public class UrlMapping
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string ShortUrl { get; set; }
        public string LongUrl { get; set; }
        public int AccessCount { get; set; }
        public DateTime LastAccessed { get; set; }
    }
}
