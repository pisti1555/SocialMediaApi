using Application.Requests.Users.Friends.Queries.GetAllOfUser;
using Application.Responses;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;
using UnitTests.Application.Users.Common;
using UnitTests.Extensions;
using UnitTests.Factories;

namespace UnitTests.Application.Users.Friends.Queries;

public class GetFriendsOfUserHandlerTest : BaseUserHandlerTest
{
    private readonly GetFriendsOfUserHandler _handler;
    
    private readonly AppUser _user;
    private readonly List<FriendshipResponseDto> _friends;

    public GetFriendsOfUserHandlerTest()
    {
        _handler = new GetFriendsOfUserHandler(UserEntityRepositoryMock.Object, FriendshipQueryRepositoryMock.Object);

        _user = TestDataFactory.CreateUser();
        var friendsList = TestDataFactory.CreateFriendships(requester: _user, count: 10);
        _friends = Mapper.Map<List<FriendshipResponseDto>>(friendsList);
    }

    [Fact]
    public async Task Handle_WhenHasFriends_ShouldReturnListOfFriends()
    {
        UserEntityRepositoryMock.SetupUserExists(_user.Id, true);
        FriendshipQueryRepositoryMock.SetupGetAll(_friends);
        
        var query = new GetFriendsOfUserQuery(_user.Id.ToString());

        var result = await _handler.Handle(query, CancellationToken.None);
        
        Assert.NotNull(result);
        Assert.Equal(_friends.Count, result.Count);
    }
    
    [Fact]
    public async Task Handle_WhenNoFriends_ShouldReturnEmptyList()
    {
        UserEntityRepositoryMock.SetupUserExists(_user.Id, true);
        FriendshipQueryRepositoryMock.SetupGetAll([]);
        
        var query = new GetFriendsOfUserQuery(_user.Id.ToString());

        var result = await _handler.Handle(query, CancellationToken.None);
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        UserEntityRepositoryMock.SetupUserExists(_user.Id, false);
        FriendshipQueryRepositoryMock.SetupGetAll([]);
        
        var query = new GetFriendsOfUserQuery(_user.Id.ToString());

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
    }
}