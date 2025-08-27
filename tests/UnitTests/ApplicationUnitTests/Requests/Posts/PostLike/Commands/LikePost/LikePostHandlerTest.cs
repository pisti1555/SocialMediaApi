using Application.Common.Interfaces.Persistence.Repositories.AppUser;
using Application.Common.Interfaces.Persistence.Repositories.Post;
using Application.Requests.Posts.PostLike.Commands.LikePost;
using ApplicationUnitTests.Common;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Moq;

namespace ApplicationUnitTests.Requests.Posts.PostLike.Commands.LikePost;

public class LikePostHandlerTest
{
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly Mock<IPostLikeRepository> _likeRepositoryMock;
    private readonly Mock<IAppUserRepository> _userRepositoryMock;
    private readonly LikePostHandler _handler;

    public LikePostHandlerTest()
    {
        _postRepositoryMock = new Mock<IPostRepository>();
        _likeRepositoryMock = new Mock<IPostLikeRepository>();
        _userRepositoryMock = new Mock<IAppUserRepository>();
        
        _postRepositoryMock.SetupGet(x => x.LikeRepository).Returns(_likeRepositoryMock.Object);

        _handler = new LikePostHandler(_postRepositoryMock.Object, _userRepositoryMock.Object, TestMapperSetup.SetupMapper());
    }

    [Fact]
    public async Task Handle_ShouldSucceed()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);

        var command = new LikePostCommand(user.Id.ToString(), post.Id.ToString());
        
        _postRepositoryMock.Setup(x => x.GetByIdAsync(post.Id))
            .ReturnsAsync(post);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
        _likeRepositoryMock.Setup(x => x.ExistsAsync(post.Id, user.Id))
            .ReturnsAsync(false);
        _postRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);
        
        var previousLastInteraction = post.LastInteraction;

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _postRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        _userRepositoryMock.Verify(x => x.GetByIdAsync(user.Id), Times.Once);
        _likeRepositoryMock.Verify(x => x.ExistsAsync(post.Id, user.Id), Times.Once);
        _likeRepositoryMock.Verify(x => x.Add(It.IsAny<Domain.Posts.PostLike>()), Times.Once);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        Assert.NotEqual(previousLastInteraction, post.LastInteraction);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_UserHasAlreadyLikedPost()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);

        var command = new LikePostCommand(user.Id.ToString(), post.Id.ToString());
        
        _postRepositoryMock.Setup(x => x.GetByIdAsync(post.Id))
            .ReturnsAsync(post);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
        _likeRepositoryMock.Setup(x => x.ExistsAsync(post.Id, user.Id))
            .ReturnsAsync(true);
        _postRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);
        
        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAnyAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        _postRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        _userRepositoryMock.Verify(x => x.GetByIdAsync(user.Id), Times.Once);
        _likeRepositoryMock.Verify(x => x.ExistsAsync(post.Id, user.Id), Times.Once);
        _likeRepositoryMock.Verify(x => x.Add(It.IsAny<Domain.Posts.PostLike>()), Times.Never);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        Assert.Equal(previousLastInteraction, post.LastInteraction);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_PostNotFound()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);

        var command = new LikePostCommand(user.Id.ToString(), post.Id.ToString());
        
        _postRepositoryMock.Setup(x => x.GetByIdAsync(post.Id))
            .ReturnsAsync((Post?)null);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
        _likeRepositoryMock.Setup(x => x.ExistsAsync(post.Id, user.Id))
            .ReturnsAsync(false);
        _postRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);
        
        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAnyAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        _postRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        _likeRepositoryMock.Verify(x => x.ExistsAsync(post.Id, user.Id), Times.Never);
        _likeRepositoryMock.Verify(x => x.Add(It.IsAny<Domain.Posts.PostLike>()), Times.Never);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        Assert.Equal(previousLastInteraction, post.LastInteraction);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_CannotSaveChanges()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);

        var command = new LikePostCommand(user.Id.ToString(), post.Id.ToString());
        
        _postRepositoryMock.Setup(x => x.GetByIdAsync(post.Id))
            .ReturnsAsync(post);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
        _likeRepositoryMock.Setup(x => x.ExistsAsync(post.Id, user.Id))
            .ReturnsAsync(false);
        _postRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAnyAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        _postRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        _likeRepositoryMock.Verify(x => x.ExistsAsync(post.Id, user.Id), Times.Once);
        _likeRepositoryMock.Verify(x => x.Add(It.IsAny<Domain.Posts.PostLike>()), Times.Once);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}