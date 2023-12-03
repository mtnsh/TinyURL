namespace TinyURL
{
    interface IUrlShortener
    {
        Task<string> GenerateShortUrl(string longUrl);
    }
}