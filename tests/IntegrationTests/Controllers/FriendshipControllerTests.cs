using System.Net;
using System.Net.Http.Json;
using API.DTOs.Bodies.Users;
using Application.Responses;
using Domain.Users;
using IntegrationTests.Common;
using IntegrationTests.Fixtures;
using IntegrationTests.Fixtures.DataFixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests.Controllers;

public class FriendshipControllerTests(CustomWebApplicationFactoryFixture factory, ITestOutputHelper output) : BaseControllerTest(factory), IAsyncLifetime
{
    private const string UsersBaseUrl = "/api/v1/users";

    private List<Friendship> _friendships = [];
    private AppUser _user = null!;
    private AppUser _otherUser = null!;
    
    [Fact]
    public async Task GetFriendsOfUser_ShouldReturnOkList()
    {
        // Arrange
        var friendCountOfUser = await DbContext.Friendships
            .AsNoTracking()
            .Where(x => x.RequesterId == _user.Id || x.ResponderId == _user.Id && x.IsConfirmed)
            .CountAsync();
        
        // Act
        var response = await Client.GetAsync($"{UsersBaseUrl}/{_user.Id}/friendships");
        var result = await response.Content.ReadFromJsonAsync<List<FriendshipResponseDto>>();
        
        // Assert response is ok and not empty
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        // Assert response contains only friends of the user
        Assert.Equal(friendCountOfUser, result.Count);
    }
    
    [Fact]
    public async Task GetFriendsOfUser_WhenNoUser_ShouldReturnNotFound()
    {
        // Act
        var response = await Client.GetAsync($"{UsersBaseUrl}/{Guid.NewGuid().ToString()}/friendships");
        
        // Assert response not found
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetFriendRequests_ShouldReturnOkList()
    {
        // Arrange
        var authClient = await GetAuthenticatedClientAsync(_user);
        var friendRequestCountOfUser = await DbContext.Friendships
            .AsNoTracking()
            .Where(x => x.ResponderId == _user.Id && !x.IsConfirmed)
            .CountAsync();
        
        // Act
        var response = await authClient.GetAsync($"{UsersBaseUrl}/{_user.Id}/friendships/requests");
        var result = await response.Content.ReadFromJsonAsync<List<FriendshipResponseDto>>();
        
        // Assert response is ok and not empty
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        // Assert response contains only friend requests of the user
        Assert.Equal(friendRequestCountOfUser, result.Count);
    }
    
    [Fact]
    public async Task AddFriend_ShouldCreateFriendRequest()
    {
        // Arrange
        var dto = new AddFriendDto(_otherUser.Id.ToString());
        var authClient = await GetAuthenticatedClientAsync(_user);
        
        // Act
        var response = await authClient.PostAsJsonAsync($"{UsersBaseUrl}/{_user.Id}/friendships", dto);
        output.WriteLine(await response.Content.ReadAsStringAsync());
        
        // Assert response is no content
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Assert friendship is present in the database
        var friendship = await DbContext.Friendships.FirstOrDefaultAsync(x =>
            x.RequesterId == _user.Id && x.ResponderId == _otherUser.Id);
        
        Assert.NotNull(friendship);
        Assert.False(friendship.IsConfirmed);
    }
    
    [Fact]
    public async Task AddFriend_WhenUserIdentityMismatch_ShouldReturnUnauthorized()
    {
        // Arrange
        var dto = new AddFriendDto(_otherUser.Id.ToString());
        var authClient = await GetAuthenticatedClientAsync(_user);
        
        // Act
        var response = await authClient.PostAsJsonAsync($"{UsersBaseUrl}/{Guid.NewGuid().ToString()}/friendships", dto);
        
        // Assert response is unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task AcceptFriendRequest_ShouldUpdateFriendshipStatusToConfirmed()
    {
        // Arrange
        var friendship = _friendships.First(f => f.ResponderId == _user.Id && !f.IsConfirmed);
        var authClient = await GetAuthenticatedClientAsync(_user);

        // Act
        var response = await authClient.PatchAsync($"{UsersBaseUrl}/{_user.Id}/friendships/{friendship.Id}/accept", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Assert friendship is updated
        var updatedFriendship = await DbContext.Friendships.AsNoTracking().FirstOrDefaultAsync(x => x.Id == friendship.Id);
        Assert.NotNull(updatedFriendship);
        Assert.True(updatedFriendship.IsConfirmed);
    }
    
    [Fact]
    public async Task DeclineFriendRequest_ShouldDeleteFriendship()
    {
        // Arrange
        var friendship = _friendships.First(f => f.ResponderId == _user.Id && !f.IsConfirmed);
        var authClient = await GetAuthenticatedClientAsync(_user);

        // Act
        var response = await authClient.PatchAsync($"{UsersBaseUrl}/{_user.Id}/friendships/{friendship.Id}/decline", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Assert friendship is deleted
        Assert.Null(await DbContext.Friendships.AsNoTracking().FirstOrDefaultAsync(x => x.Id == friendship.Id));
    }
    
    [Fact]
    public async Task DeleteFriend_ShouldRemoveFriendship()
    {
        // Arrange
        var friendship = _friendships.First(f => f.IsConfirmed && f.RequesterId == _user.Id);
        var authClient = await GetAuthenticatedClientAsync(_user);

        // Act
        var response = await authClient.DeleteAsync($"{UsersBaseUrl}/{_user.Id}/friendships/{friendship.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null(await DbContext.Friendships.AsNoTracking().FirstOrDefaultAsync(x => x.Id == friendship.Id));
    }

    public async Task InitializeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await IdentityDbContext.Database.EnsureDeletedAsync();
        
        await DbContext.Database.EnsureCreatedAsync();
        await IdentityDbContext.Database.EnsureCreatedAsync();
        
        _user = AppUserDataFixture.GetUser();
        _otherUser = AppUserDataFixture.GetUser();
        
        _friendships = FriendshipDataFixture.GetFriendships(10);
        _friendships.AddRange(FriendshipDataFixture.GetFriendships(count: 2, requester: _user, isConfirmed: false));
        _friendships.AddRange(FriendshipDataFixture.GetFriendships(count: 3, requester: _user, isConfirmed: true));
        _friendships.AddRange(FriendshipDataFixture.GetFriendships(count: 4, responder: _user, isConfirmed: false));
        _friendships.AddRange(FriendshipDataFixture.GetFriendships(count: 5, responder: _user, isConfirmed: true));
        
        DbContext.Users.Add(_otherUser);
        DbContext.Users.AddRange(_friendships.Select(x => x.Requester));
        DbContext.Users.AddRange(_friendships.Select(x => x.Responder));
        DbContext.Friendships.AddRange(_friendships);
        await DbContext.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await IdentityDbContext.Database.EnsureDeletedAsync();
    }
}