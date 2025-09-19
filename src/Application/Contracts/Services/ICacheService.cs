namespace Application.Contracts.Services;

public interface ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    public Task SetAsync<T>(string key, T value, CancellationToken ct = default);
    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default);
    public Task RemoveAsync(string key, CancellationToken ct = default);
}