using TinyURL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IUrlShortener, UrlShortener>();
builder.Services.AddSingleton<ICache<string, string>>(_ => new SimpleCache<string, string>(100)); // 100 is an example size (should be a part of configuration and have validations etc.., here for simplicity)
builder.Services.AddSingleton<IUrlService,UrlService>();

// Read MongoDB settings
var mongoDbSettings = builder.Configuration.GetSection("MongoDb").Get<MongoDbSettings>();

// Register MongoDB context
builder.Services.AddSingleton<MongoDbContext>(_ =>
    new MongoDbContext(mongoDbSettings.ConnectionString, mongoDbSettings.DatabaseName));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/tinyurl", async (string url) =>
{
    var urlShortener = app.Services.GetService<IUrlShortener>();
    return await urlShortener.GenerateShortUrl(url);
})
.WithName("TinyUrl")
.WithOpenApi();

app.MapGet("/redirectTinyUrl/{shortUrl}", async (ICache<string, string> cache, IUrlService urlService, string shortUrl) =>
{
    try //Exception handling should be in the middleware, here for simplicity
    {
        string longUrl = await cache.GetOrCreateAsync(shortUrl, async () =>
        {
            return await urlService.GetLongUrlAsync(shortUrl);
        });

        if (string.IsNullOrEmpty(longUrl))
        {
            return Results.NotFound();
        }

        return Results.Redirect(longUrl);
    }
    catch (Exception ex)
    {
        // Log and handle the exception       
        return Results.Problem("An error occurred while processing your request.");
    }
})
.WithName("RedirectTinyUrl")
.WithOpenApi();

app.Run();