using System.Data;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories.AppUser;
using Application.Common.Interfaces.Repositories.Post;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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