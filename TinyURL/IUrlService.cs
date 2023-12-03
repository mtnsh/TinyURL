namespace TinyURL
{
    public interface IUrlService
    {
        Task<string> AddUrlMappingAsync(string longUrl, string shortUrl);
        Task<string> GetLongUrlAsync(string shortUrl);
    }
}
