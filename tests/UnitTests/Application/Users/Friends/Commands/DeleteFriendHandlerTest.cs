using Application.Requests.Users.Friends.Commands.DeleteFriend;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;
using Moq;
using UnitTests.Application.Users.Common;
using UnitTests.Extensions;
using UnitTests.Factories;

namespace UnitTests.Application.Users.Friends.Commands;

public class DeleteFriendHandlerTest : BaseUserHandlerTest
{
    private readonly DeleteFriendHandler _handler;
    
    private readonly AppUser _user;
    private readonly Friendship _friendship;

    public DeleteFriendHandlerTest()
    {
        _handler = new DeleteFriendHandler(UserEntityRepositoryMock.Object, FriendshipEntityRepositoryMock.Object);
        
        _user = TestDataFactory.CreateUser();
        _friendship = TestDataFactory.CreateFriendship(_user);
    }

    [Fact]
    public async Task Handle_WhenValidRequestAndUserIsRequester_ShouldDeleteFriendship()
    {
        UserEntityRepositoryMock.SetupUserExists(_user.Id, true);
        FriendshipEntityRepositoryMock.SetupFriendship(_friendship);
        FriendshipEntityRepositoryMock.SetupSaveChanges();

        var command = new DeleteFriendCommand(_user.Id.ToString(), _friendship.Id.ToString());

        await _handler.Handle(command, CancellationToken.None);
        
        FriendshipEntityRepositoryMock.Verify(x => x.Delete(_friendship), Times.Once);
        FriendshipEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WhenValidRequestAndUserIsResponder_ShouldDeleteFriendship()
    {
        var otherValidFriendship = TestDataFactory.CreateFriendship(TestDataFactory.CreateUser(), _user);
        UserEntityRepositoryMock.SetupUserExists(_user.Id, true);
        FriendshipEntityRepositoryMock.SetupFriendship(otherValidFriendship);
        FriendshipEntityRepositoryMock.SetupSaveChanges();

        var command = new DeleteFriendCommand(_user.Id.ToString(), otherValidFriendship.Id.ToString());

        await _handler.Handle(command, CancellationToken.None);
        
        FriendshipEntityRepositoryMock.Verify(x => x.Delete(otherValidFriendship), Times.Once);
        FriendshipEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        UserEntityRepositoryMock.SetupUserExists(_user.Id, false);
        FriendshipEntityRepositoryMock.SetupFriendship(_friendship);
        FriendshipEntityRepositoryMock.SetupSaveChanges();

        var command = new DeleteFriendCommand(_user.Id.ToString(), _friendship.Id.ToString());

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        
        FriendshipEntityRepositoryMock.Verify(x => x.Delete(It.IsAny<Friendship>()), Times.Never);
        FriendshipEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenFriendshipDoesNotExist_ShouldThrowNotFoundException()
    {
        UserEntityRepositoryMock.SetupUserExists(_user.Id, true);
        FriendshipEntityRepositoryMock.SetupSaveChanges();

        var command = new DeleteFriendCommand(_user.Id.ToString(), _friendship.Id.ToString());

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        
        FriendshipEntityRepositoryMock.Verify(x => x.Delete(It.IsAny<Friendship>()), Times.Never);
        FriendshipEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenUserDoesNotOwnFriendship_ShouldThrowBadRequestException()
    {
        var someOtherFriendship = TestDataFactory.CreateFriendship(TestDataFactory.CreateUser());
        
        UserEntityRepositoryMock.SetupUserExists(_user.Id, true);
        FriendshipEntityRepositoryMock.SetupFriendship(_friendship);
        FriendshipEntityRepositoryMock.SetupFriendship(someOtherFriendship);
        FriendshipEntityRepositoryMock.SetupSaveChanges();

        var command = new DeleteFriendCommand(_user.Id.ToString(), someOtherFriendship.Id.ToString());

        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        FriendshipEntityRepositoryMock.Verify(x => x.Delete(It.IsAny<Friendship>()), Times.Never);
        FriendshipEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}