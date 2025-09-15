using Application.Requests.Posts.PostComment.Commands.RemoveCommentFromPost;
using ApplicationUnitTests.Factories;
using ApplicationUnitTests.Requests.Posts.Common;
using Domain.Common.Exceptions.CustomExceptions;
using Moq;

namespace ApplicationUnitTests.Requests.Posts.PostComment.Commands;

public class RemoveCommentFromPostHandlerTest : BasePostHandlerTest
{
    private readonly RemoveCommentFromPostHandler _handler;

    public RemoveCommentFromPostHandlerTest()
    {
        _handler = new RemoveCommentFromPostHandler(PostRepositoryMock.Object, CacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldDeleteCommentAndUpdatePost()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var comment = TestDataFactory.CreateComment(post, user);
        
        SetupPost(post);
        SetupComment(comment);
        SetupSaveChanges(true);

        var command = new RemoveCommentFromPostCommand(post.Id.ToString(), comment.Id.ToString(), user.Id.ToString());
        
        var previousLastInteraction = post.LastInteraction;

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        VerifyCommentDeleted(comment, post, previousLastInteraction);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var postId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        
        SetupUser(user);
        SetupPost(null);
        
        var command = new RemoveCommentFromPostCommand(postId.ToString(), commentId.ToString(), user.Id.ToString());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        PostRepositoryMock.Verify(r => r.GetByIdAsync(postId), Times.Once);
        CommentRepositoryMock.Verify(r => r.GetByIdAsync(commentId), Times.Never);
        CommentRepositoryMock.Verify(r => r.Delete(It.IsAny<Domain.Posts.PostComment>()), Times.Never);
        PostRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        CacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCommentDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var commentId = Guid.NewGuid();

        SetupUser(user);
        SetupPost(post);
        SetupComment(null);

        var command = new RemoveCommentFromPostCommand(post.Id.ToString(), commentId.ToString(), user.Id.ToString());
        
        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        PostRepositoryMock.Verify(r => r.GetByIdAsync(post.Id), Times.Once);
        CommentRepositoryMock.Verify(r => r.GetByIdAsync(commentId), Times.Once);
        
        VerifyCommentNotDeleted(post, previousLastInteraction);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotOwnTheComment_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var comment = TestDataFactory.CreateComment(post, user);
        var otherPost = TestDataFactory.CreatePost(user);
        
        SetupUser(user);
        SetupPost(post);
        SetupPost(otherPost);
        SetupComment(comment);
        
        var previousLastInteraction = post.LastInteraction;

        var command = new RemoveCommentFromPostCommand(otherPost.Id.ToString(), comment.Id.ToString(), user.Id.ToString());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        PostRepositoryMock.Verify(r => r.GetByIdAsync(otherPost.Id), Times.Once);
        CommentRepositoryMock.Verify(r => r.GetByIdAsync(comment.Id), Times.Once);
        
        VerifyCommentNotDeleted(post, previousLastInteraction);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotOwnTheComment_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var otherUser = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var comment = TestDataFactory.CreateComment(post, user);
        
        SetupUser(user);
        SetupUser(otherUser);
        SetupPost(post);
        SetupComment(comment);
        
        var previousLastInteraction = post.LastInteraction;

        var command = new RemoveCommentFromPostCommand(post.Id.ToString(), comment.Id.ToString(), otherUser.Id.ToString());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        PostRepositoryMock.Verify(r => r.GetByIdAsync(post.Id), Times.Once);
        CommentRepositoryMock.Verify(r => r.GetByIdAsync(comment.Id), Times.Once);
        
        VerifyCommentNotDeleted(post, previousLastInteraction);
    }

    [Fact]
    public async Task Handle_WhenCannotSaveChanges_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var comment = TestDataFactory.CreateComment(post, user);
        
        SetupUser(user);
        SetupPost(post);
        SetupComment(comment);
        SetupSaveChanges(false);

        var command = new RemoveCommentFromPostCommand(post.Id.ToString(), comment.Id.ToString(), user.Id.ToString());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        PostRepositoryMock.Verify(r => r.GetByIdAsync(post.Id), Times.Once);
        CommentRepositoryMock.Verify(r => r.GetByIdAsync(comment.Id), Times.Once);
        CommentRepositoryMock.Verify(r => r.Delete(It.IsAny<Domain.Posts.PostComment>()), Times.Once);
        PostRepositoryMock.Verify(r => r.Update(post));
        PostRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        CacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), CancellationToken.None), Times.Never);
    }
}