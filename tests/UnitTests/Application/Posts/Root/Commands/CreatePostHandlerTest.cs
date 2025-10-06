using Application.Requests.Posts.Root.Commands.CreatePost;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Domain.Users;
using Moq;
using UnitTests.Application.Posts.Common;
using UnitTests.Extensions;
using UnitTests.Factories;

namespace UnitTests.Application.Posts.Root.Commands;

public class CreatePostHandlerTest : BasePostHandlerTest
{
    private readonly CreatePostHandler _createPostHandler;

    private readonly AppUser _user;

    public CreatePostHandlerTest()
    {
        _createPostHandler = new CreatePostHandler(PostRepositoryMock.Object, UserRepositoryMock.Object, Mapper);

        _user = TestDataFactory.CreateUser();
    }
    
    private void VerifyPostAdded(bool success = true)
    {
        if (!success)
        {
            PostRepositoryMock.Verify(x => x.Add(It.IsAny<Post>()), Times.Never);
            PostRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            return;
        }
        
        PostRepositoryMock.Verify(x => x.Add(It.IsAny<Post>()), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreatePostAndSaveItToDatabase()
    {
        // Arrange
        var command = new CreatePostCommand("Test post text", _user.Id.ToString());
        
        UserRepositoryMock.SetupUser(_user, Mapper);
        PostRepositoryMock.SetupSaveChanges();

        // Act
        await _createPostHandler.Handle(command, CancellationToken.None);

        // Assert
        UserRepositoryMock.Verify(x => x.GetEntityByIdAsync(_user.Id), Times.Once);
        VerifyPostAdded();
    }

    [Fact]
    public async Task Handle_WhenTextTooLong_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new CreatePostCommand(new string('a', 20001), _user.Id.ToString());
        
        UserRepositoryMock.SetupUser(_user, Mapper);
        PostRepositoryMock.SetupSaveChanges();

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createPostHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.GetEntityByIdAsync(_user.Id), Times.Once);
        VerifyPostAdded(success: false);
    }

    [Fact]
    public async Task Handle_WhenUnparsableUserId_ShouldThrowBadRequestException()
    {
        // Arrange
        const string invalidUserId = "invalid-guid";
        var command = new CreatePostCommand("Test post text", invalidUserId);
        
        UserRepositoryMock.SetupUser(_user, Mapper);
        PostRepositoryMock.SetupSaveChanges();

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createPostHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.GetEntityByIdAsync(It.IsAny<Guid>()), Times.Never);
        VerifyPostAdded(success: false);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowBadRequestException()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var command = new CreatePostCommand("Test post text", guid.ToString());
        
        UserRepositoryMock.SetupUser(null, Mapper);
        PostRepositoryMock.SetupSaveChanges();

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createPostHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.GetEntityByIdAsync(guid), Times.Once);
        VerifyPostAdded(success: false);
    }
}