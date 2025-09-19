using Application.Requests.Posts.Root.Commands.CreatePost;
using Application.Responses;
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
        _createPostHandler = new CreatePostHandler(PostRepositoryMock.Object, UserRepositoryMock.Object, CacheServiceMock.Object, Mapper);

        _user = TestDataFactory.CreateUser();
    }
    
    private void VerifyPostAdded(bool success = true)
    {
        if (!success)
        {
            PostRepositoryMock.Verify(x => x.Add(It.IsAny<Post>()), Times.Never);
            PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
            CacheServiceMock.VerifyCacheSet<PostResponseDto?>(happened: false);
            return;
        }
        
        PostRepositoryMock.Verify(x => x.Add(It.IsAny<Post>()), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        CacheServiceMock.VerifyCacheSet<PostResponseDto?>();
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreatePostAndSaveItAndCacheIt()
    {
        // Arrange
        var command = new CreatePostCommand("Test post text", _user.Id.ToString());
        
        UserRepositoryMock.SetupUser(_user);
        PostRepositoryMock.SetupSaveChanges();

        // Act
        await _createPostHandler.Handle(command, CancellationToken.None);

        // Assert
        UserRepositoryMock.Verify(x => x.GetByIdAsync(_user.Id), Times.Once);
        VerifyPostAdded();
    }

    [Fact]
    public async Task Handle_WhenTextTooLong_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new CreatePostCommand(new string('a', 20001), _user.Id.ToString());
        
        UserRepositoryMock.SetupUser(_user);
        PostRepositoryMock.SetupSaveChanges();

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createPostHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.GetByIdAsync(_user.Id), Times.Once);
        VerifyPostAdded(success: false);
    }

    [Fact]
    public async Task Handle_WhenUnparsableUserId_ShouldThrowBadRequestException()
    {
        // Arrange
        const string invalidUserId = "invalid-guid";
        var command = new CreatePostCommand("Test post text", invalidUserId);
        
        UserRepositoryMock.SetupUser(_user);
        PostRepositoryMock.SetupSaveChanges();

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createPostHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        VerifyPostAdded(success: false);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowBadRequestException()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var command = new CreatePostCommand("Test post text", guid.ToString());
        
        UserRepositoryMock.SetupUser(null);
        PostRepositoryMock.SetupSaveChanges();

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createPostHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.GetByIdAsync(guid), Times.Once);
        VerifyPostAdded(success: false);
    }
}