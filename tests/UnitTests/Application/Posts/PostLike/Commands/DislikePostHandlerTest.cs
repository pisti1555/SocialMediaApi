using Application.Requests.Posts.PostLike.Commands.DislikePost;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Domain.Users;
using Moq;
using UnitTests.Application.Posts.Common;
using UnitTests.Extensions;
using UnitTests.Factories;
using XPostLike = Domain.Posts.PostLike;

namespace UnitTests.Application.Posts.PostLike.Commands;

public class DislikePostHandlerTest : BasePostHandlerTest
{
    private readonly DislikePostHandler _handler;
    
    private readonly AppUser _user;
    private readonly Post _post;

    public DislikePostHandlerTest()
    {
        _handler = new DislikePostHandler(PostRepositoryMock.Object, CacheServiceMock.Object);
        
        _user = TestDataFactory.CreateUser();
        _post = TestDataFactory.CreatePost(_user);
    }
    
    private void VerifyLikeDeleted(DateTime lastInteraction, bool success = true)
    {
        if (!success)
        {
            LikeRepositoryMock.Verify(x => x.Delete(It.IsAny<XPostLike>()), Times.Never);
            PostRepositoryMock.Verify(x => x.Update(_post), Times.Never);
            PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
            CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
            Assert.Equal(lastInteraction, _post.LastInteraction);
            return;
        }
        
        LikeRepositoryMock.Verify(x => x.Delete(It.IsAny<XPostLike>()), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        CacheServiceMock.VerifyCacheRemove($"post-likes-{_post.Id}");
        Assert.True(_post.LastInteraction > lastInteraction);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldDeleteLikeAndUpdatePost()
    {
        // Arrange
        var like = TestDataFactory.CreateLike(_post, _user);
        var command = new DislikePostCommand(_post.Id.ToString(), _user.Id.ToString());

        PostRepositoryMock.SetupPost(_post);
        LikeRepositoryMock.SetupLikeByUserIdAndPostId(_user.Id, _post.Id, like);
        PostRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        LikeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(_user.Id, _post.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id), Times.Once);
        
        VerifyLikeDeleted(previousLastInteraction);
    }

    [Fact]
    public async Task Handle_WhenLikeDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new DislikePostCommand(_post.Id.ToString(), _user.Id.ToString());

        PostRepositoryMock.SetupPost(_post);
        LikeRepositoryMock.SetupLikeByUserIdAndPostId(_user.Id, _post.Id, null);
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        LikeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(_user.Id, _post.Id), Times.Once);
        
        VerifyLikeDeleted(previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var like = TestDataFactory.CreateLike(_post, _user);
        var command = new DislikePostCommand(_post.Id.ToString(), _user.Id.ToString());

        PostRepositoryMock.SetupPost(null);
        LikeRepositoryMock.SetupLikeByUserIdAndPostId(_user.Id, _post.Id, like);
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        LikeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(_user.Id, _post.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id), Times.Once);
        
        VerifyLikeDeleted(previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenCannotSaveChanges_ShouldThrowBadRequestException()
    {
        // Arrange
        var like = TestDataFactory.CreateLike(_post, _user);
        var command = new DislikePostCommand(_post.Id.ToString(), _user.Id.ToString());

        PostRepositoryMock.SetupPost(_post);
        LikeRepositoryMock.SetupLikeByUserIdAndPostId(_user.Id, _post.Id, like);
        PostRepositoryMock.SetupSaveChanges(false);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        LikeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(_user.Id, _post.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id), Times.Once);
        
        LikeRepositoryMock.Verify(x => x.Delete(like), Times.Once);
        PostRepositoryMock.Verify(x => x.Update(_post), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
    }
    
    [Fact]
    public async Task Handle_WhenPostDoesNotOwnLike_ShouldThrowBadRequestException()
    {
        // Arrange
        var otherPost = TestDataFactory.CreatePost(_user);
        var like = TestDataFactory.CreateLike(otherPost, _user);
        var command = new DislikePostCommand(_post.Id.ToString(), _user.Id.ToString());

        PostRepositoryMock.SetupPost(_post);
        LikeRepositoryMock.SetupLikeByUserIdAndPostId(_user.Id, _post.Id, like);
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        LikeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(_user.Id, _post.Id), Times.Once);
        
        VerifyLikeDeleted(previousLastInteraction, false);
    }
    
    [Fact]
    public async Task Handle_WhenUserDoesNotOwnLike_ShouldThrowBadRequestException()
    {
        // Arrange
        var otherUser = TestDataFactory.CreateUser();
        var like = TestDataFactory.CreateLike(_post, otherUser);
        var command = new DislikePostCommand(_post.Id.ToString(), _user.Id.ToString());

        PostRepositoryMock.SetupPost(_post);
        LikeRepositoryMock.SetupLikeByUserIdAndPostId(_user.Id, _post.Id, like);
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        LikeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(_user.Id, _post.Id), Times.Once);
        
        VerifyLikeDeleted(previousLastInteraction, false);
    }
}