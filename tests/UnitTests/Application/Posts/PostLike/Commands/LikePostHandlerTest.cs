using System.Linq.Expressions;
using Application.Requests.Posts.PostLike.Commands.LikePost;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Domain.Users;
using Moq;
using UnitTests.Application.Posts.Common;
using UnitTests.Extensions;
using UnitTests.Factories;
using XPostLike = Domain.Posts.PostLike;

namespace UnitTests.Application.Posts.PostLike.Commands;

public class LikePostHandlerTest : BasePostHandlerTest
{
    private readonly LikePostHandler _handler;
    
    private readonly AppUser _user;
    private readonly Post _post;

    public LikePostHandlerTest()
    {
        _handler = new LikePostHandler(
            PostRepositoryMock.Object,
            UserRepositoryMock.Object, 
            LikeRepositoryMock.Object,
            CacheServiceMock.Object, 
            Mapper
        );
        
        _user = TestDataFactory.CreateUser();
        _post = TestDataFactory.CreatePost(_user);
    }
    
    private void VerifyLikeAdded(DateTime lastInteraction, bool success = true)
    {
        if (!success)
        {
            LikeRepositoryMock.Verify(x => x.Add(It.IsAny<XPostLike>()), Times.Never);
            PostRepositoryMock.Verify(x => x.Update(_post), Times.Never);
            PostRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
            Assert.Equal(lastInteraction, _post.LastInteraction);
            return;
        }
        
        LikeRepositoryMock.Verify(x => x.Add(It.IsAny<XPostLike>()), Times.Once);
        PostRepositoryMock.Verify(x => x.Update(_post), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        CacheServiceMock.VerifyCacheRemove($"post-likes-{_post.Id}");
        Assert.True(_post.LastInteraction > lastInteraction);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldAddLikeAndUpdatePost()
    {
        // Arrange
        var command = new LikePostCommand(_user.Id.ToString(), _post.Id.ToString());
        
        PostRepositoryMock.SetupPost(_post, Mapper);
        UserRepositoryMock.SetupUser(_user, Mapper);
        LikeRepositoryMock.SetupLikeExists(false);
        PostRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        CacheServiceMock.VerifyCacheRemove($"post-likes-{_post.Id.ToString()}");
        PostRepositoryMock.Verify(x => x.GetEntityByIdAsync(_post.Id), Times.Once);
        UserRepositoryMock.Verify(x => x.GetEntityByIdAsync(_user.Id), Times.Once);
        LikeRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<XPostLike, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        
        VerifyLikeAdded(previousLastInteraction);
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyLikedPost_ShouldThrowConflictException()
    {
        // Arrange
        var command = new LikePostCommand(_user.Id.ToString(), _post.Id.ToString());
        
        PostRepositoryMock.SetupPost(_post, Mapper);
        UserRepositoryMock.SetupUser(_user, Mapper);
        LikeRepositoryMock.SetupLikeExists(true);
        PostRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAnyAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));

        PostRepositoryMock.Verify(x => x.GetEntityByIdAsync(_post.Id), Times.Once);
        UserRepositoryMock.Verify(x => x.GetEntityByIdAsync(_user.Id), Times.Once);
        LikeRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<XPostLike, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        
        VerifyLikeAdded(previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new LikePostCommand(_user.Id.ToString(), _post.Id.ToString());
        
        PostRepositoryMock.SetupPost(null, Mapper);
        UserRepositoryMock.SetupUser(_user, Mapper);
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAnyAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        PostRepositoryMock.Verify(x => x.GetEntityByIdAsync(_post.Id), Times.Once);
        LikeRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<XPostLike, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
        
        VerifyLikeAdded(previousLastInteraction, false);
    }
    
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new LikePostCommand(Guid.NewGuid().ToString(), _post.Id.ToString());

        PostRepositoryMock.SetupPost(_post, Mapper);
        UserRepositoryMock.SetupUser(null, Mapper);

        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAnyAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        PostRepositoryMock.Verify(x => x.GetEntityByIdAsync(_post.Id), Times.Once);
        UserRepositoryMock.Verify(x => x.GetEntityByIdAsync(It.IsAny<Guid>()), Times.Once);
        LikeRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<XPostLike, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
        
        VerifyLikeAdded(previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenCannotSaveChanges_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new LikePostCommand(_user.Id.ToString(), _post.Id.ToString());
        
        PostRepositoryMock.SetupPost(_post, Mapper);
        UserRepositoryMock.SetupUser(_user, Mapper);
        LikeRepositoryMock.SetupLikeExists(false);
        PostRepositoryMock.SetupSaveChanges(false);

        // Act & Assert
        await Assert.ThrowsAnyAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        PostRepositoryMock.Verify(x => x.GetEntityByIdAsync(_post.Id), Times.Once);
        LikeRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<XPostLike, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        LikeRepositoryMock.Verify(x => x.Add(It.IsAny<XPostLike>()), Times.Once);
        PostRepositoryMock.Verify(x => x.Update(_post), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
    }
}