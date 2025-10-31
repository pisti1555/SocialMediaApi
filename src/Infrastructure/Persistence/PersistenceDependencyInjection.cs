using System.Data;
using Application.Contracts.Persistence.Cortex.Mediator;
using Application.Contracts.Persistence.Repositories;
using Infrastructure.Persistence.Cortex.Mediator;
using Infrastructure.Persistence.DataContext.AppDb;
using Infrastructure.Persistence.DataContext.AppIdentityDb;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Persistence.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence;

internal static class PersistenceDependencyInjection
{
    internal static IServiceCollection SetupPersistence(this IServiceCollection services, IConfiguration config)
    {
        
        
        services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseNpgsql(config.GetConnectionString("Database"));
        });
        services.AddDbContext<AppIdentityDbContext>(opt =>
        {
            opt.UseNpgsql(config.GetConnectionString("IdentityDatabase"));
        });
        
        services.AddScoped<IDbConnection>(x => 
            x.GetRequiredService<AppDbContext>().Database.GetDbConnection()
        );

        services.AddScoped<ICustomUnitOfWork, CustomUnitOfWork>();

        services.AddScoped(typeof(IRepository<>), typeof(EntityRepository<>));
        services.AddScoped(typeof(IRepository<,>), typeof(QueryRepository<,>));
        services.AddScoped(typeof(IOutsideServicesRepository<>), typeof(OutsideServicesRepository<>));
        
        return services;
    }
}