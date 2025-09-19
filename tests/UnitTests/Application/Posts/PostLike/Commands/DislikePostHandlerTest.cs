using Application.Requests.Posts.PostLike.Commands.DislikePost;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Moq;
using UnitTests.Application.Posts.Common;
using UnitTests.Extensions;
using UnitTests.Factories;
using XPostLike = Domain.Posts.PostLike;

namespace UnitTests.Application.Posts.PostLike.Commands;

public class DislikePostHandlerTest : BasePostHandlerTest
{
    private readonly DislikePostHandler _handler;

    public DislikePostHandlerTest()
    {
        _handler = new DislikePostHandler(PostRepositoryMock.Object, CacheServiceMock.Object);
    }
    
    private void VerifyLikeDeleted(Post post, DateTime lastInteraction, bool success = true)
    {
        if (!success)
        {
            LikeRepositoryMock.Verify(x => x.Delete(It.IsAny<XPostLike>()), Times.Never);
            PostRepositoryMock.Verify(x => x.Update(post), Times.Never);
            PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
            CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
            Assert.Equal(lastInteraction, post.LastInteraction);
            return;
        }
        
        LikeRepositoryMock.Verify(x => x.Delete(It.IsAny<XPostLike>()), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        CacheServiceMock.VerifyCacheRemove($"post-likes-{post.Id}");
        Assert.NotEqual(lastInteraction, post.LastInteraction);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldDeleteLikeAndUpdatePost()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var like = TestDataFactory.CreateLike(post, user);
        var command = new DislikePostCommand(post.Id.ToString(), user.Id.ToString());

        PostRepositoryMock.SetupPost(post);
        LikeRepositoryMock.SetupLikeByUserIdAndPostId(user.Id, post.Id, like);
        PostRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = post.LastInteraction;

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        LikeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        
        VerifyLikeDeleted(post, previousLastInteraction);
    }

    [Fact]
    public async Task Handle_WhenLikeDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var command = new DislikePostCommand(post.Id.ToString(), user.Id.ToString());

        PostRepositoryMock.SetupPost(post);
        LikeRepositoryMock.SetupLikeByUserIdAndPostId(user.Id, post.Id, null);
        
        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        LikeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id), Times.Once);
        
        VerifyLikeDeleted(post, previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var like = TestDataFactory.CreateLike(post, user);
        var command = new DislikePostCommand(post.Id.ToString(), user.Id.ToString());

        PostRepositoryMock.SetupPost(null);
        LikeRepositoryMock.SetupLikeByUserIdAndPostId(user.Id, post.Id, like);
        
        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        LikeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        
        VerifyLikeDeleted(post, previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenCannotSaveChanges_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var like = TestDataFactory.CreateLike(post, user);
        var command = new DislikePostCommand(post.Id.ToString(), user.Id.ToString());

        PostRepositoryMock.SetupPost(post);
        LikeRepositoryMock.SetupLikeByUserIdAndPostId(user.Id, post.Id, like);
        PostRepositoryMock.SetupSaveChanges(false);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        LikeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        
        LikeRepositoryMock.Verify(x => x.Delete(like), Times.Once);
        PostRepositoryMock.Verify(x => x.Update(post), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
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

        PostRepositoryMock.SetupPost(post);
        LikeRepositoryMock.SetupLikeByUserIdAndPostId(user.Id, post.Id, like);
        
        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        LikeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id), Times.Once);
        
        VerifyLikeDeleted(post, previousLastInteraction, false);
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

        PostRepositoryMock.SetupPost(post);
        LikeRepositoryMock.SetupLikeByUserIdAndPostId(user.Id, post.Id, like);
        
        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        LikeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id), Times.Once);
        
        VerifyLikeDeleted(post, previousLastInteraction, false);
    }
}