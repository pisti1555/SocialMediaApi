using Application.Contracts.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Hasher;

public static class HasherDependencyInjection
{
    public static IServiceCollection SetupHasher(this IServiceCollection services)
    {
        services.AddScoped<IHasher, AppHasher>();
        return services;
    }
}