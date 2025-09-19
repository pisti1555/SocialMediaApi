using Application.Contracts.Services;
using IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Persistence.DataContext;
using Xunit;

namespace IntegrationTests.Common;

public abstract class BaseControllerTest : IClassFixture<CustomWebApplicationFactoryFixture>
{
    protected readonly HttpClient Client;
    protected readonly AppDbContext DbContext;
    protected readonly ICacheService Cache;

    protected BaseControllerTest(CustomWebApplicationFactoryFixture factory)
    {
        var scope = factory.Services.CreateScope();
        Client = factory.CreateClient();
        DbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        Cache = factory.Services.GetRequiredService<ICacheService>();
    }
}