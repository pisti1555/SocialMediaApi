using System.Net;
using System.Net.Http.Json;
using Application.Common.Pagination;
using Application.Requests.Users.Root.Commands.CreateUser;
using Application.Requests.Users.Root.Commands.Login;
using Application.Responses;
using Domain.Users;
using IntegrationTests.Common;
using IntegrationTests.Fixtures;
using IntegrationTests.Fixtures.DataFixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Auth.Models;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests.Controllers;

public class UserControllerTests(CustomWebApplicationFactoryFixture factory, ITestOutputHelper output) : BaseControllerTest(factory), IAsyncLifetime
{
    private const string BaseUrl = "/api/v1/users";
    private const string ValidPassword = "Test-Password-123";
    
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
    
    private static void AssertUsersAreInSync(AppUser appUser, AppIdentityUser identityUser)
    {
        Assert.NotNull(appUser);
        Assert.NotNull(identityUser);

        Assert.Equal(appUser.Id, identityUser.Id);
        Assert.Equal(appUser.UserName, identityUser.UserName);
        Assert.Equal(appUser.Email, identityUser.Email);
    }

    [Fact]
    public async Task Login_WhenValidRequest_ShouldReturnAuthenticatedUserResponseDto()
    {
        // Arrange
        var appUser = AppUserDataFixture.GetUser();
        var identityUser = new AppIdentityUser
        {
            Id = appUser.Id,
            UserName = appUser.UserName,
            Email = appUser.Email,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        
        DbContext.Users.Add(appUser);
        await DbContext.SaveChangesAsync();
        await UserManager.CreateAsync(identityUser, ValidPassword);
        
        var savedUser = await DbContext.Users.FirstAsync();

        var command = new LoginCommand(appUser.UserName, ValidPassword);
        
        // Act
        var response = await Client.PostAsJsonAsync($"{BaseUrl}/login", command);
        output.WriteLine("Response: ");
        output.WriteLine(await response.Content.ReadAsStringAsync());
        
        var result = await response.Content.ReadFromJsonAsync<AuthenticatedUserResponseDto>();
        
        // Assert response is ok and contains user data
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        
        // Assert user data is correct
        AssertUsersMatch(savedUser, result);
        
        // Assert response contains token
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }
    
    [Fact]
    public async Task Login_WhenUserDoesNotExist_ShouldReturnUnauthorizedResponse()
    {
        // Arrange
        var appUser = AppUserDataFixture.GetUser();
        var identityUser = new AppIdentityUser
        {
            Id = appUser.Id,
            UserName = appUser.UserName,
            Email = appUser.Email
        };
        
        DbContext.Users.Add(appUser);
        await DbContext.SaveChangesAsync();
        await UserManager.CreateAsync(identityUser, ValidPassword);

        var command = new LoginCommand(appUser.UserName, "different-password");
        
        // Act
        var response = await Client.PostAsJsonAsync($"{BaseUrl}/login", command);
        output.WriteLine("Response: ");
        output.WriteLine(await response.Content.ReadAsStringAsync());
        
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        
        // Assert response is unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(401, result.Status);
    }
    
    [Fact]
    public async Task Create_WhenValidRequest_ShouldCreateUser_ThenReturnCreatedResponse_WithLocationHeader_AndAuthenticatedUserDto()
    {
        // Arrange
        var command = new CreateUserCommand(
            "test", 
            "test@email.com", 
            ValidPassword,
            "test", 
            "test", 
            "2000-01-01"
        );
        
        // Act
        var response = await Client.PostAsJsonAsync($"{BaseUrl}/register", command);
        output.WriteLine("Response: ");
        output.WriteLine(await response.Content.ReadAsStringAsync());
        var result = await response.Content.ReadFromJsonAsync<AuthenticatedUserResponseDto>();
        
        // Assert response is created and contains location header and user data
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(response.Headers.Contains("Location"));
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        
        // Assert user is created in database and in identity database (consistency is ok)
        var appUser = await DbContext.Users.FirstAsync();
        var identityUser = await IdentityDbContext.Users.FirstAsync();
        
        AssertUsersAreInSync(appUser, identityUser);
        
        // Assert location header contains the correct user id
        var locationHeaderContent = response.Headers.Location?.ToString();
        Assert.Equal(appUser.Id.ToString(), locationHeaderContent?.Split('/').Last());
        
        // Assert user data is correct
        AssertUsersMatch(appUser, result);
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
        
        await IdentityDbContext.Database.EnsureDeletedAsync();
        await IdentityDbContext.Database.EnsureCreatedAsync();

        await CreateRoles();
    }

    public async Task DisposeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await IdentityDbContext.Database.EnsureDeletedAsync();
    }
}