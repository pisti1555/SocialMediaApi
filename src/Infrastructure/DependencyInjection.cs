using Infrastructure.Auth;
using Infrastructure.Caching;
using Infrastructure.Hasher;
using Infrastructure.HealthChecks;
using Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.SetupPersistence(config);
        services.SetupHasher();
        services.SetupAuth(config);
        services.SetupCaching(config);
        services.SetupHealthChecks(config);
        
        return services;
    }
}