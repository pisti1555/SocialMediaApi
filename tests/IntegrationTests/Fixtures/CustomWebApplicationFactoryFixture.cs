using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persistence.DataContext;
using Testcontainers.PostgreSql;
using Xunit;

namespace IntegrationTests.Fixtures;

public class CustomWebApplicationFactoryFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17.1-alpine3.20")
        .WithDatabase("social_media")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = _postgresSqlContainer.GetConnectionString();
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _postgresSqlContainer.StartAsync();

        using var scope = Services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var ctx = scopedServices.GetRequiredService<AppDbContext>();

        await ctx.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgresSqlContainer.StopAsync();
    }
}