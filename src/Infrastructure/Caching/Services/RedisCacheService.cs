using Application.Contracts.Services;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Infrastructure.Caching.Services;

public sealed class RedisCacheService(IDistributedCache cache) : ICacheService
{
    private readonly TimeSpan _defaultCacheExpiry = TimeSpan.FromMinutes(10);
    
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var cached = await cache.GetStringAsync(key, ct);
        return string.IsNullOrEmpty(cached) ? 
            default : JsonConvert.DeserializeObject<T>(cached);
    }

    public async Task SetAsync<T>(string key, T value, CancellationToken ct = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _defaultCacheExpiry
        };

        var serialized = JsonConvert.SerializeObject(value);
        await cache.SetStringAsync(key, serialized, options, ct);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };

        var serialized = JsonConvert.SerializeObject(value);
        await cache.SetStringAsync(key, serialized, options, ct);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await cache.RemoveAsync(key, ct);
    }
}