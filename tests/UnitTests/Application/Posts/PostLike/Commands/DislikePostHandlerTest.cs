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
            UserRepositoryMock.Object, 
            PostRepositoryMock.Object, 
            LikeRepositoryMock.Object, 
            CacheServiceMock.Object
        );
        
        _user = TestDataFactory.CreateUser();
        _post = TestDataFactory.CreatePost(_user);
    }
    
    private void VerifyLikeDeleted(DateTime lastInteraction, bool success = true)
    {
        if (!success)
        {
            LikeRepositoryMock.Verify(x => x.Delete(It.IsAny<XPostLike>()), Times.Never);
            PostRepositoryMock.Verify(x => x.Update(_post), Times.Never);
            PostRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
            Assert.Equal(lastInteraction, _post.LastInteraction);
            return;
        }
        
        LikeRepositoryMock.Verify(x => x.Delete(It.IsAny<XPostLike>()), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        CacheServiceMock.VerifyCacheRemove($"post-likes-{_post.Id}");
        Assert.True(_post.LastInteraction > lastInteraction);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldDeleteLikeAndUpdatePostAndDeleteCache()
    {
        // Arrange
        var like = TestDataFactory.CreateLike(_post, _user);
        var command = new DislikePostCommand(_post.Id.ToString(), like.Id.ToString(), _user.Id.ToString());

        UserRepositoryMock.SetupUser(_user, Mapper);
        PostRepositoryMock.SetupPost(_post, Mapper);
        LikeRepositoryMock.SetupLike(like, Mapper);
        PostRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        UserRepositoryMock.Verify(x => x.GetEntityByIdAsync(_user.Id), Times.Once);
        LikeRepositoryMock.Verify(x => x.GetEntityByIdAsync(like.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetEntityByIdAsync(_post.Id), Times.Once);
        
        VerifyLikeDeleted(previousLastInteraction);
    }

    [Fact]
    public async Task Handle_WhenLikeDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var likeId = Guid.NewGuid();
        var command = new DislikePostCommand(_post.Id.ToString(), likeId.ToString(), _user.Id.ToString());
        
        LikeRepositoryMock.SetupLike(null, Mapper);
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        LikeRepositoryMock.Verify(x => x.GetEntityByIdAsync(likeId), Times.Once);
        
        VerifyLikeDeleted(previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var like = TestDataFactory.CreateLike(_post, _user);
        var command = new DislikePostCommand(_post.Id.ToString(), like.Id.ToString(), _user.Id.ToString());

        UserRepositoryMock.SetupUser(_user, Mapper);
        PostRepositoryMock.SetupPost(null, Mapper);
        LikeRepositoryMock.SetupLike(like, Mapper);
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        LikeRepositoryMock.Verify(x => x.GetEntityByIdAsync(like.Id), Times.Once);
        UserRepositoryMock.Verify(x => x.GetEntityByIdAsync(_user.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetEntityByIdAsync(_post.Id), Times.Once);
        
        VerifyLikeDeleted(previousLastInteraction, false);
    }
    
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var like = TestDataFactory.CreateLike(_post, _user);
        var command = new DislikePostCommand(_post.Id.ToString(), like.Id.ToString(), _user.Id.ToString());

        UserRepositoryMock.SetupUser(null, Mapper);
        PostRepositoryMock.SetupPost(_post, Mapper);
        LikeRepositoryMock.SetupLike(like, Mapper);
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        LikeRepositoryMock.Verify(x => x.GetEntityByIdAsync(like.Id), Times.Once);
        UserRepositoryMock.Verify(x => x.GetEntityByIdAsync(_user.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetEntityByIdAsync(It.IsAny<Guid>()), Times.Never);
        
        VerifyLikeDeleted(previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenCannotSaveChanges_ShouldThrowBadRequestException()
    {
        // Arrange
        var like = TestDataFactory.CreateLike(_post, _user);
        var command = new DislikePostCommand(_post.Id.ToString(), like.Id.ToString(), _user.Id.ToString());

        UserRepositoryMock.SetupUser(_user, Mapper);
        PostRepositoryMock.SetupPost(_post, Mapper);
        LikeRepositoryMock.SetupLike(like, Mapper);
        PostRepositoryMock.SetupSaveChanges(false);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        LikeRepositoryMock.Verify(x => x.GetEntityByIdAsync(like.Id), Times.Once);
        UserRepositoryMock.Verify(x => x.GetEntityByIdAsync(_user.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetEntityByIdAsync(_post.Id), Times.Once);
        
        LikeRepositoryMock.Verify(x => x.Delete(like), Times.Once);
        PostRepositoryMock.Verify(x => x.Update(_post), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
    }
    
    [Fact]
    public async Task Handle_WhenPostDoesNotOwnLike_ShouldThrowBadRequestException()
    {
        // Arrange
        var otherPost = TestDataFactory.CreatePost(_user);
        var like = TestDataFactory.CreateLike(otherPost, _user);
        var command = new DislikePostCommand(_post.Id.ToString(), like.Id.ToString(), _user.Id.ToString());
        
        LikeRepositoryMock.SetupLike(like, Mapper);
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        LikeRepositoryMock.Verify(x => x.GetEntityByIdAsync(like.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetEntityByIdAsync(It.IsAny<Guid>()), Times.Never);
        UserRepositoryMock.Verify(x => x.GetEntityByIdAsync(It.IsAny<Guid>()), Times.Never);
        
        VerifyLikeDeleted(previousLastInteraction, false);
    }
    
    [Fact]
    public async Task Handle_WhenUserDoesNotOwnLike_ShouldThrowBadRequestException()
    {
        // Arrange
        var otherUser = TestDataFactory.CreateUser();
        var like = TestDataFactory.CreateLike(_post, otherUser);
        var command = new DislikePostCommand(_post.Id.ToString(), like.Id.ToString(), _user.Id.ToString());

        UserRepositoryMock.SetupUser(_user, Mapper);
        UserRepositoryMock.SetupUser(otherUser, Mapper);
        PostRepositoryMock.SetupPost(_post, Mapper);
        LikeRepositoryMock.SetupLike(like, Mapper);
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        LikeRepositoryMock.Verify(x => x.GetEntityByIdAsync(like.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetEntityByIdAsync(It.IsAny<Guid>()), Times.Never);
        UserRepositoryMock.Verify(x => x.GetEntityByIdAsync(It.IsAny<Guid>()), Times.Never);
        
        VerifyLikeDeleted(previousLastInteraction, false);
    }
}