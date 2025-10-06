using Application.Contracts.Services;
using Infrastructure.Caching.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Caching;

internal static class CachingDependencyInjection
{
    internal static IServiceCollection SetupCaching(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Redis");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Redis connection string is missing.");
        }
        
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
            options.InstanceName = "SocialMediaApi";
        });

        services.AddSingleton<ICacheService, RedisCacheService>();
        
        return services;
    }
}