using Application.Requests.Posts.PostLike.Commands.LikePost;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Moq;
using UnitTests.Application.Posts.Common;
using UnitTests.Extensions;
using UnitTests.Factories;
using XPostLike = Domain.Posts.PostLike;

namespace UnitTests.Application.Posts.PostLike.Commands;

public class LikePostHandlerTest : BasePostHandlerTest
{
    private readonly LikePostHandler _handler;

    public LikePostHandlerTest()
    {
        _handler = new LikePostHandler(
            PostRepositoryMock.Object,
            UserRepositoryMock.Object, 
            CacheServiceMock.Object, 
            Mapper
        );
    }
    
    private void VerifyLikeAdded(Post post, DateTime lastInteraction, bool success = true)
    {
        if (!success)
        {
            LikeRepositoryMock.Verify(x => x.Add(It.IsAny<XPostLike>()), Times.Never);
            PostRepositoryMock.Verify(x => x.Update(post), Times.Never);
            PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
            CacheServiceMock.VerifyCacheRemove($"post-likes-{post.Id}", false);
            Assert.Equal(lastInteraction, post.LastInteraction);
            return;
        }
        
        LikeRepositoryMock.Verify(x => x.Add(It.IsAny<XPostLike>()), Times.Once);
        PostRepositoryMock.Verify(x => x.Update(post), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        CacheServiceMock.VerifyCacheRemove($"post-likes-{post.Id}");
        Assert.NotEqual(lastInteraction, post.LastInteraction);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldAddLikeAndUpdatePost()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var command = new LikePostCommand(user.Id.ToString(), post.Id.ToString());
        
        PostRepositoryMock.SetupPost(post);
        UserRepositoryMock.SetupUser(user);
        LikeRepositoryMock.SetupLikeExists(post.Id, user.Id, false);
        PostRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = post.LastInteraction;

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        CacheServiceMock.VerifyCacheRemove($"post-likes-{post.Id.ToString()}");
        PostRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        UserRepositoryMock.Verify(x => x.GetByIdAsync(user.Id), Times.Once);
        LikeRepositoryMock.Verify(x => x.ExistsAsync(post.Id, user.Id), Times.Once);
        
        VerifyLikeAdded(post, previousLastInteraction);
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyLikedPost_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var command = new LikePostCommand(user.Id.ToString(), post.Id.ToString());
        
        PostRepositoryMock.SetupPost(post);
        UserRepositoryMock.SetupUser(user);
        LikeRepositoryMock.SetupLikeExists(post.Id, user.Id, true);
        PostRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAnyAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        PostRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        UserRepositoryMock.Verify(x => x.GetByIdAsync(user.Id), Times.Once);
        LikeRepositoryMock.Verify(x => x.ExistsAsync(post.Id, user.Id), Times.Once);
        
        VerifyLikeAdded(post, previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var command = new LikePostCommand(user.Id.ToString(), post.Id.ToString());
        
        PostRepositoryMock.SetupPost(null);
        UserRepositoryMock.SetupUser(user);
        
        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAnyAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        PostRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        LikeRepositoryMock.Verify(x => x.ExistsAsync(post.Id, user.Id), Times.Never);
        
        VerifyLikeAdded(post, previousLastInteraction, false);
    }
    
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var command = new LikePostCommand(Guid.NewGuid().ToString(), post.Id.ToString());

        PostRepositoryMock.SetupPost(post);
        UserRepositoryMock.SetupUser(null);

        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAnyAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        PostRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        UserRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
        LikeRepositoryMock.Verify(x => x.ExistsAsync(post.Id, user.Id), Times.Never);
        
        VerifyLikeAdded(post, previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenCannotSaveChanges_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var command = new LikePostCommand(user.Id.ToString(), post.Id.ToString());
        
        PostRepositoryMock.SetupPost(post);
        UserRepositoryMock.SetupUser(user);
        LikeRepositoryMock.SetupLikeExists(post.Id, user.Id, false);
        PostRepositoryMock.SetupSaveChanges(false);

        // Act & Assert
        await Assert.ThrowsAnyAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        PostRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        LikeRepositoryMock.Verify(x => x.ExistsAsync(post.Id, user.Id), Times.Once);
        LikeRepositoryMock.Verify(x => x.Add(It.IsAny<global::Domain.Posts.PostLike>()), Times.Once);
        PostRepositoryMock.Verify(x => x.Update(post), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
    }
}