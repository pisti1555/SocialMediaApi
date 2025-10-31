using Application.Requests.Users.Friends.Commands.AcceptFriendRequest;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;
using Moq;
using UnitTests.Application.Users.Common;
using UnitTests.Extensions;
using UnitTests.Factories;

namespace UnitTests.Application.Users.Friends.Commands;

public class AcceptFriendRequestHandlerTest : BaseUserHandlerTest
{
    private readonly AcceptFriendRequestHandler _handler;
    
    private readonly AppUser _user;
    private readonly Friendship _friendship;

    public AcceptFriendRequestHandlerTest()
    {
        _handler = new AcceptFriendRequestHandler(UserEntityRepositoryMock.Object, FriendshipEntityRepositoryMock.Object);
        
        var otherUser = TestDataFactory.CreateUser();
        _user = TestDataFactory.CreateUser();
        
        _friendship = TestDataFactory.CreateFriendship(otherUser, _user);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldConfirmFriendship()
    {
        UserEntityRepositoryMock.SetupUserExists(_user.Id, true);
        FriendshipEntityRepositoryMock.SetupFriendship(_friendship);
        FriendshipEntityRepositoryMock.SetupSaveChanges();
        
        var command = new AcceptFriendRequestCommand(_user.Id.ToString(), _friendship.Id.ToString());
        
        await _handler.Handle(command, CancellationToken.None);
        
        FriendshipEntityRepositoryMock.Verify(x => x.Update(_friendship), Times.Once);
        FriendshipEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        UserEntityRepositoryMock.SetupUserExists(_user.Id, false);
        FriendshipEntityRepositoryMock.SetupFriendship(_friendship);
        FriendshipEntityRepositoryMock.SetupSaveChanges();
        
        var command = new AcceptFriendRequestCommand(_user.Id.ToString(), _friendship.Id.ToString());
        
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        
        FriendshipEntityRepositoryMock.Verify(x => x.Update(_friendship), Times.Never);
        FriendshipEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenFriendshipDoesNotExist_ShouldThrowNotFoundException()
    {
        UserEntityRepositoryMock.SetupUserExists(_user.Id, true);
        FriendshipEntityRepositoryMock.SetupSaveChanges();
        
        var command = new AcceptFriendRequestCommand(_user.Id.ToString(), _friendship.Id.ToString());
        
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        
        FriendshipEntityRepositoryMock.Verify(x => x.Update(_friendship), Times.Never);
        FriendshipEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenFriendshipAlreadyConfirmed_ShouldThrowBadRequestException()
    {
        var someUser = TestDataFactory.CreateUser();
        var confirmedFriendship = TestDataFactory.CreateFriendship(someUser, _user, isConfirmed: true);
        
        UserEntityRepositoryMock.SetupUserExists(_user.Id, true);
        FriendshipEntityRepositoryMock.SetupFriendship(confirmedFriendship);
        FriendshipEntityRepositoryMock.SetupSaveChanges();
        
        var command = new AcceptFriendRequestCommand(_user.Id.ToString(), confirmedFriendship.Id.ToString());
        
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        FriendshipEntityRepositoryMock.Verify(x => x.Update(confirmedFriendship), Times.Never);
        FriendshipEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenUserIsNotOnResponderSideOfFriendRequest_ShouldThrowBadRequestException()
    {
        var someUser = TestDataFactory.CreateUser();
        var ownFriendship = TestDataFactory.CreateFriendship(_user, someUser);
        
        UserEntityRepositoryMock.SetupUserExists(_user.Id, true);
        FriendshipEntityRepositoryMock.SetupFriendship(ownFriendship);
        FriendshipEntityRepositoryMock.SetupSaveChanges();
        
        var command = new AcceptFriendRequestCommand(_user.Id.ToString(), ownFriendship.Id.ToString());
        
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        FriendshipEntityRepositoryMock.Verify(x => x.Update(ownFriendship), Times.Never);
        FriendshipEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}