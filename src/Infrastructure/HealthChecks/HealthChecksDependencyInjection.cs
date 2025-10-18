using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.HealthChecks;

internal static class HealthChecksDependencyInjection
{
    internal static IServiceCollection SetupHealthChecks(this IServiceCollection services, IConfiguration config)
    {
        var appDb = config.GetConnectionString("Database") 
                    ?? throw new InvalidOperationException("Missing 'Database' connection string.");
        var identityDb = config.GetConnectionString("IdentityDatabase") 
                    ?? throw new InvalidOperationException("Missing 'IdentityDatabase' connection string.");
        var redis = config.GetConnectionString("Redis") 
                    ?? throw new InvalidOperationException("Missing 'Redis' connection string.");
        
        services.AddHealthChecks()
            .AddNpgSql(appDb, name: "AppDb")
            .AddNpgSql(identityDb, name: "IdentityDb")
            .AddRedis(redis, name: "Redis");

        return services;
    }
}