using System.Data;
using Application.Common.Interfaces.Persistence.Cortex.Mediator;
using Application.Common.Interfaces.Persistence.Repositories.AppUser;
using Application.Common.Interfaces.Persistence.Repositories.Post;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Cortex.Mediator;
using Persistence.DataContext;
using Persistence.Repositories;
using Persistence.Repositories.Post;

namespace Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseNpgsql(config.GetConnectionString("DefaultConnection"));
        });
        
        services.AddScoped<IDbConnection>(sp => 
            sp.GetRequiredService<AppDbContext>().Database.GetDbConnection()
        );

        services.AddScoped<ICustomUnitOfWork, CustomUnitOfWork>();
        
        services.AddScoped<IAppUserRepository, AppUserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IPostCommentRepository, PostCommentRepository>();
        services.AddScoped<IPostLikeRepository, PostLikeRepository>();
        
        return services;
    }
}