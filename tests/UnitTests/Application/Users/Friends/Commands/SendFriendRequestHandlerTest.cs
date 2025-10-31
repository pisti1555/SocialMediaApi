using Application.Requests.Users.Friends.Commands.SendFriendRequest;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;
using Moq;
using UnitTests.Application.Users.Common;
using UnitTests.Extensions;
using UnitTests.Factories;

namespace UnitTests.Application.Users.Friends.Commands;

public class SendFriendRequestHandlerTest : BaseUserHandlerTest
{
    private readonly SendFriendRequestHandler _handler;
    private readonly AppUser _user;
    private readonly AppUser _userToAdd;

    public SendFriendRequestHandlerTest()
    {
        _handler = new SendFriendRequestHandler(UserEntityRepositoryMock.Object, FriendshipEntityRepositoryMock.Object);
        _user = TestDataFactory.CreateUser();
        _userToAdd = TestDataFactory.CreateUser();
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldSaveFriendship()
    {
        var userId = _user.Id.ToString();
        var userToAddId = _userToAdd.Id.ToString();

        UserEntityRepositoryMock.SetupUserExists(_user.Id, true);
        UserEntityRepositoryMock.SetupUserExists(_userToAdd.Id, true);
        
        FriendshipEntityRepositoryMock.SetupFriendshipExistsByAnyFilters(false);
        FriendshipEntityRepositoryMock.SetupSaveChanges();
        
        var command = new SendFriendRequestCommand(userId, userToAddId);
        
        await _handler.Handle(command, CancellationToken.None);
        
        FriendshipEntityRepositoryMock.Verify(x => x.Add(It.IsAny<Friendship>()), Times.Once);
        FriendshipEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WhenSameUserId_ShouldThrowBadRequestException()
    {
        var userId = _user.Id.ToString();

        UserEntityRepositoryMock.SetupUserExists(_user.Id, true);
        
        FriendshipEntityRepositoryMock.SetupFriendshipExistsByAnyFilters(false);
        FriendshipEntityRepositoryMock.SetupSaveChanges();
        
        var command = new SendFriendRequestCommand(userId, userId);
        
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        FriendshipEntityRepositoryMock.Verify(x => x.Add(It.IsAny<Friendship>()), Times.Never);
        FriendshipEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenUserToAddDoesNotExist_ShouldThrowNotFoundException()
    {
        var userId = _user.Id.ToString();
        var userToAddId = _userToAdd.Id.ToString();

        UserEntityRepositoryMock.SetupUserExists(_user.Id, true);
        UserEntityRepositoryMock.SetupUserExists(_userToAdd.Id, false);
        
        FriendshipEntityRepositoryMock.SetupFriendshipExistsByAnyFilters(false);
        FriendshipEntityRepositoryMock.SetupSaveChanges();
        
        var command = new SendFriendRequestCommand(userId, userToAddId);
        
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        
        FriendshipEntityRepositoryMock.Verify(x => x.Add(It.IsAny<Friendship>()), Times.Never);
        FriendshipEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenCurrentUserNotExist_ShouldThrowNotFoundException()
    {
        var userId = _user.Id.ToString();
        var userToAddId = _userToAdd.Id.ToString();

        UserEntityRepositoryMock.SetupUserExists(_user.Id, false);
        UserEntityRepositoryMock.SetupUserExists(_userToAdd.Id, true);
        
        FriendshipEntityRepositoryMock.SetupFriendshipExistsByAnyFilters(false);
        FriendshipEntityRepositoryMock.SetupSaveChanges();
        
        var command = new SendFriendRequestCommand(userId, userToAddId);
        
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        
        FriendshipEntityRepositoryMock.Verify(x => x.Add(It.IsAny<Friendship>()), Times.Never);
        FriendshipEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenFriendshipAlreadyExists_ShouldThrowBadRequestException()
    {
        var userId = _user.Id.ToString();
        var userToAddId = _userToAdd.Id.ToString();

        UserEntityRepositoryMock.SetupUserExists(_user.Id, true);
        UserEntityRepositoryMock.SetupUserExists(_userToAdd.Id, true);
        
        FriendshipEntityRepositoryMock.SetupFriendshipExistsByAnyFilters(true);
        FriendshipEntityRepositoryMock.SetupSaveChanges();
        
        var command = new SendFriendRequestCommand(userId, userToAddId);
        
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        FriendshipEntityRepositoryMock.Verify(x => x.Add(It.IsAny<Friendship>()), Times.Never);
        FriendshipEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}