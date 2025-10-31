using Application.Requests.Users.Friends.Queries.GetAllRequestsOfUser;
using Application.Responses;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;
using UnitTests.Application.Users.Common;
using UnitTests.Extensions;
using UnitTests.Factories;

namespace UnitTests.Application.Users.Friends.Queries;

public class GetFriendRequestsOfUserHandlerTest : BaseUserHandlerTest
{
    private readonly GetFriendRequestsOfUserHandler _handler;
    
    private readonly AppUser _user;
    private readonly List<FriendshipResponseDto> _friendRequests;

    public GetFriendRequestsOfUserHandlerTest()
    {
        _handler = new GetFriendRequestsOfUserHandler(UserEntityRepositoryMock.Object, FriendshipQueryRepositoryMock.Object);

        _user = TestDataFactory.CreateUser();
        var friendRequestsList = TestDataFactory.CreateFriendships(requester: _user, count: 5);
        _friendRequests = Mapper.Map<List<FriendshipResponseDto>>(friendRequestsList);
    }
    
    [Fact]
    public async Task Handle_WhenHasFriendRequests_ShouldReturnListOfFriendRequests()
    {
        UserEntityRepositoryMock.SetupUserExists(_user.Id, true);
        FriendshipQueryRepositoryMock.SetupGetAll(_friendRequests);
        
        var query = new GetFriendRequestsOfUserQuery(_user.Id.ToString());

        var result = await _handler.Handle(query, CancellationToken.None);
        
        Assert.NotNull(result);
        Assert.Equal(_friendRequests.Count, result.Count);
    }
    
    [Fact]
    public async Task Handle_WhenNoFriendRequest_ShouldReturnEmptyList()
    {
        UserEntityRepositoryMock.SetupUserExists(_user.Id, true);
        FriendshipQueryRepositoryMock.SetupGetAll([]);
        
        var query = new GetFriendRequestsOfUserQuery(_user.Id.ToString());

        var result = await _handler.Handle(query, CancellationToken.None);
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        UserEntityRepositoryMock.SetupUserExists(_user.Id, false);
        FriendshipEntityRepositoryMock.SetupGetAll([]);
        
        var query = new GetFriendRequestsOfUserQuery(_user.Id.ToString());

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
    }
}