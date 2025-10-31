using Application.Requests.Posts.Root.Commands.UpdatePost;
using Application.Responses;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Domain.Users;
using Moq;
using UnitTests.Application.Posts.Common;
using UnitTests.Extensions;
using UnitTests.Factories;

namespace UnitTests.Application.Posts.Root.Commands;

public class UpdatePostHandlerTest : BasePostHandlerTest
{
    private readonly UpdatePostHandler _updatePostHandler;
    
    private readonly AppUser _user;
    private readonly Post _post;
    
    public UpdatePostHandlerTest()
    {
        _updatePostHandler = new UpdatePostHandler(UserEntityRepositoryMock.Object, PostEntityRepositoryMock.Object, CacheServiceMock.Object, Mapper);
        
        _user = TestDataFactory.CreateUser();
        _post = TestDataFactory.CreatePost(_user);
    }
    
    private void VerifyPostUpdated(bool success = true)
    {
        if (!success)
        {
            PostEntityRepositoryMock.Verify(x => x.Update(It.IsAny<Post>()), Times.Never);
            PostEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            CacheServiceMock.VerifyCacheSet<PostResponseDto?>(happened: false);
            return;
        }
        
        PostEntityRepositoryMock.Verify(x => x.Update(It.IsAny<Post>()), Times.Once);
        PostEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        CacheServiceMock.VerifyCacheSet<PostResponseDto?>();
    }
    
    [Fact]
    public async Task Handle_WhenValidRequest_ShouldUpdatePostAndSaveItAndCacheIt()
    {
        // Arrange
        var command = new UpdatePostCommand(_post.Id.ToString(), _user.Id.ToString(), "Updated post text");
        
        UserEntityRepositoryMock.SetupUser(_user);
        PostEntityRepositoryMock.SetupPost(_post);
        PostEntityRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act
        await _updatePostHandler.Handle(command, CancellationToken.None);

        // Assert
        VerifyPostUpdated();
        Assert.True(_post.LastInteraction > previousLastInteraction);
    }
    
    [Fact]
    public async Task Handle_WhenUserDoesNotOwnPost_ShouldThrowBadRequestException()
    {
        // Arrange
        var otherUser = TestDataFactory.CreateUser();
        
        var command = new UpdatePostCommand(_post.Id.ToString(), otherUser.Id.ToString(), "Updated post text");
        
        UserEntityRepositoryMock.SetupUser(_user);
        UserEntityRepositoryMock.SetupUser(otherUser);
        PostEntityRepositoryMock.SetupPost(_post);
        PostEntityRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _updatePostHandler.Handle(command, CancellationToken.None));
        
        VerifyPostUpdated(success: false);
        Assert.Equal(previousLastInteraction, _post.LastInteraction);
    }
    
    [Fact]
    public async Task Handle_WhenTextTooLong_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new UpdatePostCommand(_post.Id.ToString(), _user.Id.ToString(), new string('a', 20001));
        
        UserEntityRepositoryMock.SetupUser(_user);
        PostEntityRepositoryMock.SetupPost(_post);
        PostEntityRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _updatePostHandler.Handle(command, CancellationToken.None));
        
        VerifyPostUpdated(success: false);
        Assert.Equal(previousLastInteraction, _post.LastInteraction);
    }
    
    [Fact]
    public async Task Handle_WhenUnparsableUserId_ShouldThrowBadRequestException()
    {
        // Arrange
        const string invalidUserId = "invalid-guid";
        var command = new UpdatePostCommand(_post.Id.ToString(), invalidUserId, "Updated post text");
        
        UserEntityRepositoryMock.SetupUser(_user);
        PostEntityRepositoryMock.SetupPost(_post);
        PostEntityRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _updatePostHandler.Handle(command, CancellationToken.None));
        
        UserEntityRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        PostEntityRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id, It.IsAny<CancellationToken>()), Times.Never);
        VerifyPostUpdated(success: false);
        Assert.Equal(previousLastInteraction, _post.LastInteraction);
    }
    
    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowBadRequestException()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var command = new UpdatePostCommand(_post.Id.ToString(), guid.ToString(), "Updated post text");
        
        UserEntityRepositoryMock.SetupUser(null);
        PostEntityRepositoryMock.SetupPost(_post);
        PostEntityRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _updatePostHandler.Handle(command, CancellationToken.None));
        
        VerifyPostUpdated(success: false);
        Assert.Equal(previousLastInteraction, _post.LastInteraction);
    }
    
    [Fact]
    public async Task Handle_WhenPostNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var command = new UpdatePostCommand(guid.ToString(), _user.Id.ToString(), "Updated post text");
        
        UserEntityRepositoryMock.SetupUser(_user);
        PostEntityRepositoryMock.SetupPost(null);
        PostEntityRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _updatePostHandler.Handle(command, CancellationToken.None));
        
        VerifyPostUpdated(success: false);
        Assert.Equal(previousLastInteraction, _post.LastInteraction);
    }
}