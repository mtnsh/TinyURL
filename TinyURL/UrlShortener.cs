using System.Security.Cryptography;
using System.Text;

namespace TinyURL
{
    public class UrlShortener : IUrlShortener
    {
        private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const int ShortUrlLength = 8; // You can adjust this length
        private const int maxEncodedLength = 32; // Adjust based on the maximum expected length of the encoded string
        private readonly ICache<string, string> _cache;
        private readonly IUrlService _urlService;

        public UrlShortener(ICache<string, string> cache, IUrlService urlService)
        {
            _cache = cache;
            _urlService = urlService;
        }

        public async Task<string> GenerateShortUrl(string longUrl)
        {
            // Check cache first
            var cachedShortUrl = _cache.Get(longUrl);
            if (!string.IsNullOrEmpty(cachedShortUrl))
            {
                return cachedShortUrl;
            }

            // Generate short URL
            var shortUrl = GenerateShortUrlFromLongUrl(longUrl);

            // Check and handle collisions
            var existingLongUrl = await _urlService.GetLongUrlAsync(shortUrl);
            while (existingLongUrl != null && existingLongUrl != longUrl)
            {
                shortUrl = GenerateShortUrlFromLongUrl(longUrl); // Regenerate
                existingLongUrl = await _urlService.GetLongUrlAsync(shortUrl);
            }

            // Save to MongoDB
            await _urlService.AddUrlMappingAsync(longUrl, shortUrl);

            // Update cache
            _cache.Set(longUrl, shortUrl);

            return shortUrl;
        }

        private string GenerateShortUrlFromLongUrl(string longUrl)
        {
            var hash = CreateMd5Hash(longUrl);
            var shortUrl = EncodeToBase(hash).Substring(0, ShortUrlLength);
            return shortUrl;
        }

        private byte[] CreateMd5Hash(string input)
        {
            return MD5.HashData(Encoding.UTF8.GetBytes(input));
        }

        private string EncodeToBase(byte[] hash)
        {
            Span<char> buffer = stackalloc char[maxEncodedLength];

            int bufferIndex = 0;
            foreach (byte b in hash)
            {
                buffer[bufferIndex++] = Alphabet[b % Alphabet.Length];
            }

            return new string(buffer.Slice(0, bufferIndex));
        }
    }
}
