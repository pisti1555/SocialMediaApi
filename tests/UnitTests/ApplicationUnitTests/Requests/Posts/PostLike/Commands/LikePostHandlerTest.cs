using Application.Requests.Posts.PostLike.Commands.LikePost;
using ApplicationUnitTests.Factories;
using ApplicationUnitTests.Requests.Posts.Common;
using Domain.Common.Exceptions.CustomExceptions;
using Moq;

namespace ApplicationUnitTests.Requests.Posts.PostLike.Commands;

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

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldAddLikeAndUpdatePost()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var command = new LikePostCommand(user.Id.ToString(), post.Id.ToString());
        
        SetupPost(post);
        SetupUser(user);
        SetupLikeExists(post.Id, user.Id, false);
        SetupSaveChanges(true);
        
        var previousLastInteraction = post.LastInteraction;

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
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
        
        SetupPost(post);
        SetupUser(user);
        SetupLikeExists(post.Id, user.Id, true);
        SetupSaveChanges(true);
        
        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAnyAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        PostRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        UserRepositoryMock.Verify(x => x.GetByIdAsync(user.Id), Times.Once);
        LikeRepositoryMock.Verify(x => x.ExistsAsync(post.Id, user.Id), Times.Once);
        
        VerifyLikeNotAdded(post, previousLastInteraction);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var command = new LikePostCommand(user.Id.ToString(), post.Id.ToString());
        
        SetupPost(null);
        SetupUser(user);
        
        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAnyAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        PostRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        LikeRepositoryMock.Verify(x => x.ExistsAsync(post.Id, user.Id), Times.Never);
        
        VerifyLikeNotAdded(post, previousLastInteraction);
    }
    
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var command = new LikePostCommand(Guid.NewGuid().ToString(), post.Id.ToString());

        SetupPost(post);
        SetupUser(null);

        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAnyAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        PostRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        UserRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
        LikeRepositoryMock.Verify(x => x.ExistsAsync(post.Id, user.Id), Times.Never);
        
        VerifyLikeNotAdded(post, previousLastInteraction);
    }


    [Fact]
    public async Task Handle_WhenCannotSaveChanges_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var command = new LikePostCommand(user.Id.ToString(), post.Id.ToString());
        
        SetupPost(post);
        SetupUser(user);
        SetupLikeExists(post.Id, user.Id, false);
        SetupSaveChanges(false);

        // Act & Assert
        await Assert.ThrowsAnyAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        PostRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        LikeRepositoryMock.Verify(x => x.ExistsAsync(post.Id, user.Id), Times.Once);
        LikeRepositoryMock.Verify(x => x.Add(It.IsAny<Domain.Posts.PostLike>()), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        CacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), CancellationToken.None), Times.Never);
    }
}