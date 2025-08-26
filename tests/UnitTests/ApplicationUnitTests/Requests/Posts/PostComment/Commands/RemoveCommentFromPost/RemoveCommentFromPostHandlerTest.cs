using Application.Common.Interfaces.Repositories.Post;
using Application.Requests.Posts.PostComment.Commands.RemoveCommentFromPost;
using ApplicationUnitTests.Common;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Moq;

namespace ApplicationUnitTests.Requests.Posts.PostComment.Commands.RemoveCommentFromPost;

public class RemoveCommentFromPostHandlerTest
{
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly Mock<IPostCommentRepository> _commentRepositoryMock;
    private readonly RemoveCommentFromPostHandler _handler;

    public RemoveCommentFromPostHandlerTest()
    {
        _postRepositoryMock = new Mock<IPostRepository>();
        _commentRepositoryMock = new Mock<IPostCommentRepository>();

        _postRepositoryMock.SetupGet(r => r.CommentRepository)
                           .Returns(_commentRepositoryMock.Object);

        _handler = new RemoveCommentFromPostHandler(_postRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSucceed()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);
        var comment = TestObjects.CreateTestComment(user, post);

        _postRepositoryMock.Setup(r => r.GetByIdAsync(post.Id)).ReturnsAsync(post);
        _commentRepositoryMock.Setup(r => r.GetByIdAsync(comment.Id)).ReturnsAsync(comment);
        _postRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var command = new RemoveCommentFromPostCommand(post.Id.ToString(), comment.Id.ToString(), user.Id.ToString());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _commentRepositoryMock.Verify(r => r.Delete(comment), Times.Once);
        _postRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_PostNotFound()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        var command = new RemoveCommentFromPostCommand(postId.ToString(), commentId.ToString(), userId.ToString());

        _postRepositoryMock.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync((Post?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_CommentNotFound()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);
        var commentId = Guid.NewGuid();

        _postRepositoryMock.Setup(x => x.GetByIdAsync(post.Id)).ReturnsAsync(post);
        _commentRepositoryMock.Setup(x => x.GetByIdAsync(commentId)).ReturnsAsync((Domain.Posts.PostComment?)null);

        var command = new RemoveCommentFromPostCommand(post.Id.ToString(), commentId.ToString(), user.Id.ToString());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_PostDoesNotOwnTheComment()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);
        var comment = TestObjects.CreateTestComment(user, post);

        var otherPost = TestObjects.CreateTestPost(user);

        _postRepositoryMock.Setup(x => x.GetByIdAsync(otherPost.Id)).ReturnsAsync(otherPost);
        _postRepositoryMock.Setup(x => x.GetByIdAsync(post.Id)).ReturnsAsync(post);
        _commentRepositoryMock.Setup(x => x.GetByIdAsync(comment.Id)).ReturnsAsync(comment);

        var command = new RemoveCommentFromPostCommand(otherPost.Id.ToString(), comment.Id.ToString(), user.Id.ToString());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_UserDoesNotOwnTheComment()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);
        var comment = TestObjects.CreateTestComment(user, post);
        
        var otherUserId = Guid.NewGuid();

        _postRepositoryMock.Setup(x => x.GetByIdAsync(post.Id)).ReturnsAsync(post);
        _commentRepositoryMock.Setup(x => x.GetByIdAsync(comment.Id)).ReturnsAsync(comment);

        var command = new RemoveCommentFromPostCommand(post.Id.ToString(), comment.Id.ToString(), otherUserId.ToString());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_CannotSaveChanges()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);
        var comment = TestObjects.CreateTestComment(user, post);

        _postRepositoryMock.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync(post);
        _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId)).ReturnsAsync(comment);
        _postRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(false);

        var command = new RemoveCommentFromPostCommand(postId.ToString(), commentId.ToString(), userId.ToString());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
    }
}