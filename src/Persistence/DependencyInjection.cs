using System.Data;
using Application.Contracts.Persistence.Cortex.Mediator;
using Application.Contracts.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Cortex.Mediator;
using Persistence.DataContext;
using Persistence.Repositories;

namespace Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseNpgsql(config.GetConnectionString("Database"));
        });
        services.AddDbContext<AppIdentityDbContext>(opt =>
        {
            opt.UseNpgsql(config.GetConnectionString("IdentityDatabase"));
        });
        
        services.AddScoped<IDbConnection>(sp => 
            sp.GetRequiredService<AppDbContext>().Database.GetDbConnection()
        );

        services.AddScoped<ICustomUnitOfWork, CustomUnitOfWork>();

        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        
        return services;
    }
}