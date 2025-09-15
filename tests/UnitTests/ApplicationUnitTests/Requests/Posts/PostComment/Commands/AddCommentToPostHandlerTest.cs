using Application.Requests.Posts.PostComment.Commands.AddCommentToPost;
using ApplicationUnitTests.Factories;
using ApplicationUnitTests.Requests.Posts.Common;
using Domain.Common.Exceptions.CustomExceptions;
using Moq;

namespace ApplicationUnitTests.Requests.Posts.PostComment.Commands;

public class AddCommentToPostHandlerTest : BasePostHandlerTest
{
    private readonly AddCommentToPostHandler _handler;

    public AddCommentToPostHandlerTest()
    {
        _handler = new AddCommentToPostHandler(
            PostRepositoryMock.Object, 
            UserRepositoryMock.Object, 
            CacheServiceMock.Object, 
            Mapper
        );
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldAddCommentAndUpdatePost()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var command = new AddCommentToPostCommand(post.Id.ToString(), user.Id.ToString(), "Test comment");

        SetupPost(post);
        SetupUser(user);
        SetupSaveChanges(true);
        
        var previousLastInteraction = post.LastInteraction;

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        UserRepositoryMock.Verify(x => x.GetByIdAsync(user.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        
        VerifyCommentAdded(post, previousLastInteraction);
    }

    [Fact]
    public async Task Handle_WhenUnparsableUserId_ShouldThrowBadRequestException()
    {
        // Arrange
        var post = TestDataFactory.CreatePost();
        const string invalidUserId = "invalid-user-guid";
        var command = new AddCommentToPostCommand(post.Id.ToString(), invalidUserId, "Test comment");
        
        SetupPost(post);
        SetupUser(TestDataFactory.CreateUser());
        SetupSaveChanges(true);

        var previousLastInteraction = post.LastInteraction;
        
        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        UserRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        
        VerifyCommentNotAdded(post, previousLastInteraction);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var post = TestDataFactory.CreatePost();
        var notExistingUserId = Guid.NewGuid();
        var command = new AddCommentToPostCommand(post.Id.ToString(), notExistingUserId.ToString(), "Test comment");

        SetupUser(null);
        SetupPost(post);
        SetupSaveChanges(true);
        
        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        UserRepositoryMock.Verify(x => x.GetByIdAsync(notExistingUserId), Times.Once);
        
        VerifyCommentNotAdded(post, previousLastInteraction);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var notExistingPostId = Guid.NewGuid();
        var command = new AddCommentToPostCommand(notExistingPostId.ToString(), user.Id.ToString(), "Test comment");

        SetupUser(user);
        SetupPost(null);
        SetupSaveChanges(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        UserRepositoryMock.Verify(x => x.GetByIdAsync(user.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetByIdAsync(notExistingPostId), Times.Once);
        
        CommentRepositoryMock.Verify(x => x.Add(It.IsAny<Domain.Posts.PostComment>()), Times.Never);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        CacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCannotSaveChanges_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var command = new AddCommentToPostCommand(post.Id.ToString(), user.Id.ToString(), "Test comment");
        
        SetupPost(post);
        SetupUser(user);
        SetupSaveChanges(false);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.GetByIdAsync(user.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);

        CommentRepositoryMock.Verify(x => x.Add(It.IsAny<Domain.Posts.PostComment>()), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        CacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), CancellationToken.None), Times.Never);
    }
}