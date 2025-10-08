using System.Net;
using System.Net.Http.Json;
using Application.Common.Pagination;
using Application.Responses;
using Domain.Users;
using IntegrationTests.Common;
using IntegrationTests.Fixtures;
using IntegrationTests.Fixtures.DataFixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests.Controllers;

public class UserControllerTests(CustomWebApplicationFactoryFixture factory, ITestOutputHelper output) : BaseControllerTest(factory), IAsyncLifetime
{
    private const string BaseUrl = "/api/v1/users";
    
    private static void AssertUsersMatch(AppUser expected, UserResponseDto actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.UserName, actual.UserName);
        Assert.Equal(expected.Email, actual.Email);
        Assert.Equal(expected.FirstName, actual.FirstName);
        Assert.Equal(expected.LastName, actual.LastName);
        Assert.Equal(expected.DateOfBirth, actual.DateOfBirth);
    }
    
    [Fact]
    public async Task GetById_ShouldCacheUser_ThenReturnUserResponseDto()
    {
        // Arrange
        DbContext.Users.AddRange(AppUserDataFixture.GetUser());
        await DbContext.SaveChangesAsync();
        
        var user = await DbContext.Users.FirstAsync();
        
        // Act
        var response = await Client.GetAsync($"{BaseUrl}/{user.Id}");
        var result = await response.Content.ReadFromJsonAsync<UserResponseDto>();
        
        // Assert response is ok and contains user data
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        
        // Assert user data is cached
        var cachedUser = await Cache.GetAsync<UserResponseDto>($"user-{user.Id.ToString()}");
        Assert.NotNull(cachedUser);

        // Assert user data is correct
        AssertUsersMatch(user, result);
    }
    
    [Fact]
    public async Task GetById_WhenUserNotFound_ShouldReturnNotFoundResponse()
    {
        var response = await Client.GetAsync($"{BaseUrl}/{Guid.NewGuid().ToString()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetAllPaged_WhenRequestedPage1AndPageSize19_ShouldReturnListWith19Users()
    {
        // Arrange
        DbContext.Users.AddRange(AppUserDataFixture.GetUsers(25));
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"{BaseUrl}?pageNumber=1&pageSize=19");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<UserResponseDto>>();
        
        // Assert response is ok and contains user data
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        
        // Assert pagination headers are present
        Assert.True(response.Headers.Contains("X-Current-Page"));
        Assert.True(response.Headers.Contains("X-Page-Size"));
        Assert.True(response.Headers.Contains("X-Total-Items"));
        Assert.True(response.Headers.Contains("X-Total-Pages"));
        
        // Assert result is correct
        Assert.NotEmpty(result.Items);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(19, result.PageSize);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
    }
    
    [Fact]
    public async Task GetAllPaged_WhenNoUserFound_ShouldReturnEmptyList()
    {
        // Act
        var response = await Client.GetAsync(BaseUrl);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<UserResponseDto>>();
        
        // Assert response is ok and contains user data
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);

        // Assert pagination headers are present
        Assert.True(response.Headers.Contains("X-Current-Page"));
        Assert.True(response.Headers.Contains("X-Page-Size"));
        Assert.True(response.Headers.Contains("X-Total-Items"));
        Assert.True(response.Headers.Contains("X-Total-Pages"));
        
        // Assert result is correct
        Assert.Empty(result.Items);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
    }

    public async Task InitializeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
    }
}