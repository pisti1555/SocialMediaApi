using Application.Contracts.Services;
using Infrastructure.Hasher.Configuration;
using Infrastructure.Hasher.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Hasher;

public static class HasherDependencyInjection
{
    public static IServiceCollection SetupHasher(this IServiceCollection services)
    {
        services.AddOptions<HasherConfiguration>()
            .BindConfiguration("Hasher")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddScoped<IHasher, AppHasher>();
        
        return services;
    }
}