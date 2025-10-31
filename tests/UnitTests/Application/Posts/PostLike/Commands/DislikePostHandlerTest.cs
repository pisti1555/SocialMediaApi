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
        _handler = new DislikePostHandler(
            UserEntityRepositoryMock.Object, 
            PostEntityRepositoryMock.Object, 
            LikeEntityRepositoryMock.Object, 
            CacheServiceMock.Object
        );
        
        _user = TestDataFactory.CreateUser();
        _post = TestDataFactory.CreatePost(_user);
    }
    
    private void VerifyLikeDeleted(DateTime lastInteraction, bool success = true)
    {
        if (!success)
        {
            LikeEntityRepositoryMock.Verify(x => x.Delete(It.IsAny<XPostLike>()), Times.Never);
            PostEntityRepositoryMock.Verify(x => x.Update(_post), Times.Never);
            PostEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
            Assert.Equal(lastInteraction, _post.LastInteraction);
            return;
        }
        
        LikeEntityRepositoryMock.Verify(x => x.Delete(It.IsAny<XPostLike>()), Times.Once);
        PostEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        CacheServiceMock.VerifyCacheRemove($"post-likes-{_post.Id}");
        Assert.True(_post.LastInteraction > lastInteraction);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldDeleteLikeAndUpdatePostAndDeleteCache()
    {
        // Arrange
        var like = TestDataFactory.CreateLike(_post, _user);
        var command = new DislikePostCommand(_post.Id.ToString(), like.Id.ToString(), _user.Id.ToString());

        UserEntityRepositoryMock.SetupUser(_user);
        PostEntityRepositoryMock.SetupPost(_post);
        LikeEntityRepositoryMock.SetupLike(like);
        PostEntityRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        UserEntityRepositoryMock.Verify(x => x.GetByIdAsync(_user.Id, It.IsAny<CancellationToken>()), Times.Once);
        LikeEntityRepositoryMock.Verify(x => x.GetByIdAsync(like.Id, It.IsAny<CancellationToken>()), Times.Once);
        PostEntityRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id, It.IsAny<CancellationToken>()), Times.Once);
        
        VerifyLikeDeleted(previousLastInteraction);
    }

    [Fact]
    public async Task Handle_WhenLikeDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var likeId = Guid.NewGuid();
        var command = new DislikePostCommand(_post.Id.ToString(), likeId.ToString(), _user.Id.ToString());
        
        LikeEntityRepositoryMock.SetupLike(null);
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        LikeEntityRepositoryMock.Verify(x => x.GetByIdAsync(likeId, It.IsAny<CancellationToken>()), Times.Once);
        
        VerifyLikeDeleted(previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var like = TestDataFactory.CreateLike(_post, _user);
        var command = new DislikePostCommand(_post.Id.ToString(), like.Id.ToString(), _user.Id.ToString());

        UserEntityRepositoryMock.SetupUser(_user);
        PostEntityRepositoryMock.SetupPost(null);
        LikeEntityRepositoryMock.SetupLike(like);
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        LikeEntityRepositoryMock.Verify(x => x.GetByIdAsync(like.Id, It.IsAny<CancellationToken>()), Times.Once);
        UserEntityRepositoryMock.Verify(x => x.GetByIdAsync(_user.Id, It.IsAny<CancellationToken>()), Times.Once);
        PostEntityRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id, It.IsAny<CancellationToken>()), Times.Once);
        
        VerifyLikeDeleted(previousLastInteraction, false);
    }
    
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var like = TestDataFactory.CreateLike(_post, _user);
        var command = new DislikePostCommand(_post.Id.ToString(), like.Id.ToString(), _user.Id.ToString());

        UserEntityRepositoryMock.SetupUser(null);
        PostEntityRepositoryMock.SetupPost(_post);
        LikeEntityRepositoryMock.SetupLike(like);
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        LikeEntityRepositoryMock.Verify(x => x.GetByIdAsync(like.Id, It.IsAny<CancellationToken>()), Times.Once);
        UserEntityRepositoryMock.Verify(x => x.GetByIdAsync(_user.Id, It.IsAny<CancellationToken>()), Times.Once);
        PostEntityRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        
        VerifyLikeDeleted(previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenCannotSaveChanges_ShouldThrowBadRequestException()
    {
        // Arrange
        var like = TestDataFactory.CreateLike(_post, _user);
        var command = new DislikePostCommand(_post.Id.ToString(), like.Id.ToString(), _user.Id.ToString());

        UserEntityRepositoryMock.SetupUser(_user);
        PostEntityRepositoryMock.SetupPost(_post);
        LikeEntityRepositoryMock.SetupLike(like);
        PostEntityRepositoryMock.SetupSaveChanges(false);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        LikeEntityRepositoryMock.Verify(x => x.GetByIdAsync(like.Id, It.IsAny<CancellationToken>()), Times.Once);
        UserEntityRepositoryMock.Verify(x => x.GetByIdAsync(_user.Id, It.IsAny<CancellationToken>()), Times.Once);
        PostEntityRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id, It.IsAny<CancellationToken>()), Times.Once);
        
        LikeEntityRepositoryMock.Verify(x => x.Delete(like), Times.Once);
        PostEntityRepositoryMock.Verify(x => x.Update(_post), Times.Once);
        PostEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
    }
    
    [Fact]
    public async Task Handle_WhenPostDoesNotOwnLike_ShouldThrowBadRequestException()
    {
        // Arrange
        var otherPost = TestDataFactory.CreatePost(_user);
        var like = TestDataFactory.CreateLike(otherPost, _user);
        var command = new DislikePostCommand(_post.Id.ToString(), like.Id.ToString(), _user.Id.ToString());
        
        LikeEntityRepositoryMock.SetupLike(like);
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        LikeEntityRepositoryMock.Verify(x => x.GetByIdAsync(like.Id, It.IsAny<CancellationToken>()), Times.Once);
        PostEntityRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        UserEntityRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        
        VerifyLikeDeleted(previousLastInteraction, false);
    }
    
    [Fact]
    public async Task Handle_WhenUserDoesNotOwnLike_ShouldThrowBadRequestException()
    {
        // Arrange
        var otherUser = TestDataFactory.CreateUser();
        var like = TestDataFactory.CreateLike(_post, otherUser);
        var command = new DislikePostCommand(_post.Id.ToString(), like.Id.ToString(), _user.Id.ToString());

        UserEntityRepositoryMock.SetupUser(_user);
        UserEntityRepositoryMock.SetupUser(otherUser);
        PostEntityRepositoryMock.SetupPost(_post);
        LikeEntityRepositoryMock.SetupLike(like);
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        LikeEntityRepositoryMock.Verify(x => x.GetByIdAsync(like.Id, It.IsAny<CancellationToken>()), Times.Once);
        PostEntityRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        UserEntityRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        
        VerifyLikeDeleted(previousLastInteraction, false);
    }
}