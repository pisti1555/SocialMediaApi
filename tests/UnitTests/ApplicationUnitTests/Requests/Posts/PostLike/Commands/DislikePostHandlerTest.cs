using Application.Requests.Posts.PostLike.Commands.DislikePost;
using ApplicationUnitTests.Factories;
using ApplicationUnitTests.Requests.Posts.Common;
using Domain.Common.Exceptions.CustomExceptions;
using Moq;

namespace ApplicationUnitTests.Requests.Posts.PostLike.Commands;

public class DislikePostHandlerTest : BasePostHandlerTest
{
    private readonly DislikePostHandler _handler;

    public DislikePostHandlerTest()
    {
        _handler = new DislikePostHandler(PostRepositoryMock.Object, CacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldDeleteLikeAndUpdatePost()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var like = TestDataFactory.CreateLike(post, user);
        var command = new DislikePostCommand(post.Id.ToString(), user.Id.ToString());

        SetupPost(post);
        SetupLikeByUserIdAndPostId(user.Id, post.Id, like);
        SetupSaveChanges(true);
        
        var previousLastInteraction = post.LastInteraction;

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        LikeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        
        VerifyLikeDeleted(like, post, previousLastInteraction);
    }

    [Fact]
    public async Task Handle_WhenLikeDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var command = new DislikePostCommand(post.Id.ToString(), user.Id.ToString());

        SetupPost(post);
        SetupLikeByUserIdAndPostId(user.Id, post.Id, null);
        
        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        LikeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id), Times.Once);
        
        VerifyLikeNotDeleted(post, previousLastInteraction);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var like = TestDataFactory.CreateLike(post, user);
        var command = new DislikePostCommand(post.Id.ToString(), user.Id.ToString());

        SetupPost(null);
        SetupLikeByUserIdAndPostId(user.Id, post.Id, like);
        
        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        LikeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        
        VerifyLikeNotDeleted(post, previousLastInteraction);
    }

    [Fact]
    public async Task Handle_WhenCannotSaveChanges_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var like = TestDataFactory.CreateLike(post, user);
        var command = new DislikePostCommand(post.Id.ToString(), user.Id.ToString());

        SetupPost(post);
        SetupLikeByUserIdAndPostId(user.Id, post.Id, like);
        SetupSaveChanges(false);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        LikeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        LikeRepositoryMock.Verify(x => x.Delete(like), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        CacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), CancellationToken.None), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenPostDoesNotOwnLike_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var otherPost = TestDataFactory.CreatePost(user);
        var like = TestDataFactory.CreateLike(otherPost, user);
        var command = new DislikePostCommand(post.Id.ToString(), user.Id.ToString());

        SetupPost(post);
        SetupLikeByUserIdAndPostId(user.Id, post.Id, like);
        
        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        LikeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id), Times.Once);
        
        VerifyLikeNotDeleted(post, previousLastInteraction);
    }
    
    [Fact]
    public async Task Handle_WhenUserDoesNotOwnLike_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var otherUser = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var like = TestDataFactory.CreateLike(post, otherUser);
        var command = new DislikePostCommand(post.Id.ToString(), user.Id.ToString());

        SetupPost(post);
        SetupLikeByUserIdAndPostId(user.Id, post.Id, like);
        
        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        LikeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id), Times.Once);
        
        VerifyLikeNotDeleted(post, previousLastInteraction);
    }
}