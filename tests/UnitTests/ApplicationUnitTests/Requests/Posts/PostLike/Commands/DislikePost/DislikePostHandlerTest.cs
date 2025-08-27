using Application.Common.Interfaces.Persistence.Repositories.Post;
using Application.Requests.Posts.PostLike.Commands.DislikePost;
using ApplicationUnitTests.Common;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Moq;

namespace ApplicationUnitTests.Requests.Posts.PostLike.Commands.DislikePost;

public class DislikePostHandlerTest
{
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly Mock<IPostLikeRepository> _likeRepositoryMock;
    private readonly DislikePostHandler _handler;

    public DislikePostHandlerTest()
    {
        _postRepositoryMock = new Mock<IPostRepository>();
        _likeRepositoryMock = new Mock<IPostLikeRepository>();

        _postRepositoryMock.SetupGet(x => x.LikeRepository).Returns(_likeRepositoryMock.Object);

        _handler = new DislikePostHandler(_postRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSucceed()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);
        var like = TestObjects.CreateTestLike(user, post);

        var command = new DislikePostCommand(post.Id.ToString(), user.Id.ToString());

        _likeRepositoryMock.Setup(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id))
            .ReturnsAsync(like);
        _postRepositoryMock.Setup(x => x.GetByIdAsync(post.Id))
            .ReturnsAsync(post);
        _postRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);
        
        var previousLastInteraction = post.LastInteraction;

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _likeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id), Times.Once);
        _postRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        _likeRepositoryMock.Verify(x => x.Delete(like), Times.Once);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        Assert.NotEqual(previousLastInteraction, post.LastInteraction);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_LikeNotFound()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);
        var like = TestObjects.CreateTestLike(user, post);

        var command = new DislikePostCommand(post.Id.ToString(), user.Id.ToString());

        _likeRepositoryMock.Setup(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id))
            .ReturnsAsync((Domain.Posts.PostLike?)null);
        _postRepositoryMock.Setup(x => x.GetByIdAsync(post.Id))
            .ReturnsAsync(post);
        _postRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);
        
        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        _likeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id), Times.Once);
        _likeRepositoryMock.Verify(x => x.Delete(like), Times.Never);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        Assert.Equal(previousLastInteraction, post.LastInteraction);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_PostNotFound()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);
        var like = TestObjects.CreateTestLike(user, post);

        var command = new DislikePostCommand(post.Id.ToString(), user.Id.ToString());

        _likeRepositoryMock.Setup(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id))
            .ReturnsAsync(like);
        _postRepositoryMock.Setup(x => x.GetByIdAsync(post.Id))
            .ReturnsAsync((Post?)null);
        _postRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);
        
        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        _likeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id), Times.Once);
        _postRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        _likeRepositoryMock.Verify(x => x.Delete(like), Times.Never);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        Assert.Equal(previousLastInteraction, post.LastInteraction);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_CannotSaveChanges()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);
        var like = TestObjects.CreateTestLike(user, post);

        var command = new DislikePostCommand(post.Id.ToString(), user.Id.ToString());

        _likeRepositoryMock.Setup(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id))
            .ReturnsAsync(like);
        _postRepositoryMock.Setup(x => x.GetByIdAsync(post.Id))
            .ReturnsAsync(post);
        _postRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        _likeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id), Times.Once);
        _postRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        _likeRepositoryMock.Verify(x => x.Delete(like), Times.Once);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    
    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_PostDoesNotOwnTheLike()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);
        var otherPost = TestObjects.CreateTestPost(user);
        var like = TestObjects.CreateTestLike(user, otherPost);

        var command = new DislikePostCommand(post.Id.ToString(), user.Id.ToString());

        _likeRepositoryMock.Setup(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id))
            .ReturnsAsync(like);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        _likeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id), Times.Once);
        _likeRepositoryMock.Verify(x => x.Delete(like), Times.Never);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }
    
    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_UserDoesNotOwnTheLike()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var otherUser = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);
        var like = TestObjects.CreateTestLike(otherUser, post);

        var command = new DislikePostCommand(post.Id.ToString(), user.Id.ToString());

        _likeRepositoryMock.Setup(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id))
            .ReturnsAsync(like);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        _likeRepositoryMock.Verify(x => x.GetByUserIdAndPostIdAsync(user.Id, post.Id), Times.Once);
        _likeRepositoryMock.Verify(x => x.Delete(like), Times.Never);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }
}