using Microsoft.Extensions.Caching.Hybrid;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromSeconds(10),
        LocalCacheExpiration = TimeSpan.FromSeconds(10)
    };
});

var app = builder.Build();

app.MapOpenApi();

app.MapScalarApiReference(options =>
{
    // Fluent API
    options
        .WithTitle("Hybrid cache API")
        .WithModels(false)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
        .WithTheme(ScalarTheme.Kepler);
});

app.UseHttpsRedirection();

app.MapGet("/get-cached-string", async (HybridCache cache, CancellationToken token) =>
{
    string myKey = "my-key";
    string value = await GetStringAsync(cache, myKey, token);
    
    return Results.Ok(value);
})
.WithName("GetCachedString");

app.MapGet("/get-cached-item", async (HybridCache cache, CancellationToken token) =>
    {
        string myKey = "my-key";
        MyItem value = await GetValueAsync(cache, myKey, token);
    
        return Results.Ok(value);
    })
    .WithName("GetCachedItem");

app.Run();


async Task<string> GetStringAsync(HybridCache cache, string key, CancellationToken token = default)
{
    return await cache.GetOrCreateAsync(
        key,
        async cancel => await GetStringFromDbAsync(cancel),
        cancellationToken: token
    );
}

async Task<MyItem> GetValueAsync(HybridCache cache, string key, CancellationToken token = default)
{
    return await cache.GetOrCreateAsync(
        key,
        async cancel => await GetValueFromDbAsync(cancel),
        cancellationToken: token
    );
}

Task<string> GetStringFromDbAsync(CancellationToken token)
{
    return Task.FromResult(DateTime.UtcNow.ToString("dd/MM/yyyy - HH:mm:ss"));
}

Task<MyItem> GetValueFromDbAsync(CancellationToken token)
{
    return Task.FromResult(new MyItem
    {
        Id = Guid.NewGuid()
    });
}


class MyItem
{
    public Guid Id { get; set; }
}