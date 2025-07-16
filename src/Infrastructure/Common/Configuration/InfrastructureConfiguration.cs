using Application.Common.Interfaces.Repositories;
using Infrastructure.Persistence.DataContext;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Common.Configuration;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseSqlite(config.GetConnectionString("SQLiteConnection"));
        });
        
        services.AddScoped<IAppUserRepository, AppUserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        
        return services;   
    }
}