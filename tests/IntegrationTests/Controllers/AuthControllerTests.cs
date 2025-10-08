using System.Net;
using System.Net.Http.Json;
using API.DTOs.Bodies.Auth;
using Application.Requests.Auth.Commands.Login;
using Application.Requests.Auth.Commands.Registration;
using Application.Responses;
using Domain.Users;
using Infrastructure.Auth.Models;
using IntegrationTests.Common;
using IntegrationTests.Fixtures;
using IntegrationTests.Fixtures.DataFixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests.Controllers;

public class AuthControllerTests(CustomWebApplicationFactoryFixture factory, ITestOutputHelper output) : BaseControllerTest(factory), IAsyncLifetime
{
    private const string BaseUrl = "/api/v1/auth";
    private const string ValidPassword = "Test-Password-123";

    private async Task<AppUser> SetupUserAsync(AppUser? user = null)
    {
        var appUser = user ?? AppUserDataFixture.GetUser();
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
        await UserManager.AddToRoleAsync(identityUser, "User");
        await IdentityDbContext.SaveChangesAsync();
        return await DbContext.Users.FirstAsync(x => x.Id == appUser.Id);
    }
    
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
        var user = await SetupUserAsync();

        var command = new LoginCommand(user.UserName, ValidPassword, false);
        
        // Act
        var response = await Client.PostAsJsonAsync($"{BaseUrl}/login", command);
        output.WriteLine(await response.Content.ReadAsStringAsync());
        var result = await response.Content.ReadFromJsonAsync<AuthenticatedUserResponseDto>();
        
        // Assert response is ok and contains user data
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        
        // Assert user data is correct
        AssertUsersMatch(user, result);
        
        // Assert response contains token
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
    }
    
    [Fact]
    public async Task Login_WhenUserDoesNotExist_ShouldReturnUnauthorizedResponse()
    {
        // Arrange
        var user = await SetupUserAsync();

        var command = new LoginCommand(user.UserName, "different-password", false);
        
        // Act
        var response = await Client.PostAsJsonAsync($"{BaseUrl}/login", command);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        
        // Assert response is unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(401, result.Status);
    }
    
    [Fact]
    public async Task Register_WhenValidRequest_ShouldCreateUser_ThenReturnCreatedResponse_WithLocationHeader_AndAuthenticatedUserDto()
    {
        // Arrange
        var command = new RegistrationCommand(
            "test", 
            "test@email.com", 
            ValidPassword,
            "test", 
            "test", 
            "2000-01-01",
            false
        );
        
        // Act
        var response = await Client.PostAsJsonAsync($"{BaseUrl}/register", command);
        var result = await response.Content.ReadFromJsonAsync<AuthenticatedUserResponseDto>();
        
        // Assert response is created and contains location header and user data
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(response.Headers.Contains("Location"));
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        
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
    public async Task RefreshAccess_WhenValidRequest_ShouldReturnNewTokens()
    {
        // Arrange
        var registrationDto = new RegistrationDto(
            "test", 
            "test@email.com", 
            ValidPassword,
            "test", 
            "test", 
            "2000-01-01",
            false
        );
        
        var registrationResponse = await Client.PostAsJsonAsync($"{BaseUrl}/register", registrationDto);
        var registrationResult = await registrationResponse.Content.ReadFromJsonAsync<AuthenticatedUserResponseDto>();
        
        var refreshDto = new RefreshAccessDto(
            AccessToken: registrationResult?.AccessToken, 
            RefreshToken: registrationResult?.RefreshToken
        );
        
        // Act
        var refreshResponse = await Client.PostAsJsonAsync($"{BaseUrl}/refresh-access", refreshDto);
        var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<TokenResponseDto>();
        
        // Assert response is ok and contains new tokens
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        Assert.NotNull(refreshResult);
        Assert.False(string.IsNullOrWhiteSpace(refreshResult.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshResult.RefreshToken));
        Assert.NotEqual(registrationResult?.AccessToken, refreshResult?.AccessToken);
        Assert.NotEqual(registrationResult?.RefreshToken, refreshResult?.RefreshToken);
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