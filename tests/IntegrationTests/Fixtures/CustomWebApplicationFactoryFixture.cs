using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persistence.DataContext;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace IntegrationTests.Fixtures;

public class CustomWebApplicationFactoryFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17.6")
        .WithDatabase("social_media")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:8.2.1")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var postgresConnectionString = _postgresSqlContainer.GetConnectionString();
        var redisConnectionString = _redisContainer.GetConnectionString();
        
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services =>
        {
            // Reassign the connection string to the test database
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(postgresConnectionString);
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
        await _postgresSqlContainer.StartAsync();
        await _redisContainer.StartAsync();
        
        using var scope = Services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var ctx = scopedServices.GetRequiredService<AppDbContext>();
        await ctx.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgresSqlContainer.StopAsync();
        await _redisContainer.StopAsync();
    }
}