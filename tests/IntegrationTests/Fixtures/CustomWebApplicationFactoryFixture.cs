using Infrastructure.Auth.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persistence.DataContext;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace IntegrationTests.Fixtures;

public class CustomWebApplicationFactoryFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17.6")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:8.2.1")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
        
        var postgresBaseConnection = _postgreSqlContainer.GetConnectionString();
        var redisConnectionString = _redisContainer.GetConnectionString();
        
        base.ConfigureWebHost(builder);
        
        builder.ConfigureTestServices(services =>
        {
            services.Configure<JwtConfiguration>(config.GetSection("Jwt"));
            
            // Reassign the connection string to the test database
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions<AppIdentityDbContext>>();
            
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql($"{postgresBaseConnection};Database=social_media;");
            });
            services.AddDbContext<AppIdentityDbContext>(options =>
            {
                options.UseNpgsql($"{postgresBaseConnection};Database=social_media_identity;");
            });
            
            // Reassign the connection string to redis cache
            services.RemoveAll<IDistributedCache>();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "SocialMediaApi";
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        await _redisContainer.StartAsync();
        
        using var scope = Services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        
        var ctx = scopedServices.GetRequiredService<AppDbContext>();
        var identityCtx = scopedServices.GetRequiredService<AppIdentityDbContext>();
        
        await ctx.Database.EnsureCreatedAsync();
        await identityCtx.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgreSqlContainer.StopAsync();
        await _redisContainer.StopAsync();
    }
}