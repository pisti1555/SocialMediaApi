using System.Net;
using System.Net.Http.Json;
using Application.Common.Pagination;
using Application.Requests.Users.Root.Commands.CreateUser;
using Application.Responses;
using IntegrationTests.Common;
using IntegrationTests.Fixtures;
using IntegrationTests.Fixtures.DataFixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IntegrationTests.Controllers;

public class UserControllerTests(CustomWebApplicationFactoryFixture factory) : BaseControllerTest(factory), IAsyncLifetime
{
    private const string BaseUrl = "/api/users";
    
    [Fact]
    public async Task Create_WhenValidRequest_ShouldCreateAndCacheUser_ThenReturnCreatedResponse_WithLocationHeader_AndUserDto()
    {
        var command = new CreateUserCommand("test", "test@email.com", "test", "test", DateOnly.Parse("2000-01-01"));
        
        var response = await Client.PostAsJsonAsync(BaseUrl, command);
        var result = await response.Content.ReadFromJsonAsync<UserResponseDto>();
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(response.Headers.Contains("Location"));
        
        var firstFoundUserInDb = await DbContext.Users.FirstAsync();
        var locationHeaderContent = response.Headers.Location?.ToString();
        
        var cachedUser = await Cache.GetAsync<UserResponseDto>($"user-{firstFoundUserInDb.Id.ToString()}");
        Assert.NotNull(cachedUser);
        
        Assert.Equal(firstFoundUserInDb.Id.ToString(), locationHeaderContent?.Split('/').Last());
        Assert.NotNull(result);
        
        Assert.Equal(firstFoundUserInDb.Id, result.Id);
        Assert.Equal(firstFoundUserInDb.UserName, result.UserName);
        Assert.Equal(firstFoundUserInDb.Email, result.Email);
        Assert.Equal(firstFoundUserInDb.FirstName, result.FirstName);
        Assert.Equal(firstFoundUserInDb.LastName, result.LastName);
        Assert.Equal(firstFoundUserInDb.DateOfBirth, result.DateOfBirth);
    }
    
    [Fact]
    public async Task GetById_ShouldCacheUser_ThenReturnUserResponseDto()
    {
        DbContext.Users.AddRange(AppUserDataFixture.GetUser());
        await DbContext.SaveChangesAsync();
        
        var user = await DbContext.Users.FirstAsync();
        var userId = user.Id;
        
        var response = await Client.GetAsync($"{BaseUrl}/{userId}");
        var result = await response.Content.ReadFromJsonAsync<UserResponseDto>();
        
        var cachedUser = await Cache.GetAsync<UserResponseDto>($"user-{userId.ToString()}");
        Assert.NotNull(cachedUser);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal(user.UserName, result.UserName);
        Assert.Equal(user.Email, result.Email);
    }
    
    [Fact]
    public async Task GetById_WhenUserNotFound_ShouldReturnNotFoundResponse()
    {
        var response = await Client.GetAsync($"{BaseUrl}/{Guid.NewGuid().ToString()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetAllPaged_WhenRequestedPage1AndPageSize19_ShouldCacheResult_ThenReturnListWith19Users()
    {
        DbContext.Users.AddRange(AppUserDataFixture.GetUsers(25));
        await DbContext.SaveChangesAsync();
        
        var response = await Client.GetAsync($"{BaseUrl}?pageNumber=1&pageSize=19");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<UserResponseDto>>();
        
        var cachedResult = await Cache.GetAsync<PagedResult<UserResponseDto>>("users-1-19");
        Assert.NotNull(cachedResult);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Current-Page"));
        Assert.True(response.Headers.Contains("X-Page-Size"));
        Assert.True(response.Headers.Contains("X-Total-Items"));
        Assert.True(response.Headers.Contains("X-Total-Pages"));
        
        Assert.NotNull(result);
        
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(19, result.PageSize);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        
        Assert.NotEmpty(result.Items);
    }
    
    [Fact]
    public async Task GetAllPaged_WhenNoUserFound_ShouldCacheResult_ThenReturnEmptyList()
    {
        var response = await Client.GetAsync(BaseUrl);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<UserResponseDto>>();
        
        var cachedResult = await Cache.GetAsync<PagedResult<UserResponseDto>>("users-1-10");
        Assert.NotNull(cachedResult);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Current-Page"));
        Assert.True(response.Headers.Contains("X-Page-Size"));
        Assert.True(response.Headers.Contains("X-Total-Items"));
        Assert.True(response.Headers.Contains("X-Total-Pages"));
        
        Assert.NotNull(result);
        
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
        
        Assert.Empty(result.Items);
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