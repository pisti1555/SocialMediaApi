using Application.Requests.Posts.PostComment.Commands.RemoveCommentFromPost;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Moq;
using UnitTests.Application.Posts.Common;
using UnitTests.Extensions;
using UnitTests.Factories;
using XPostComment = Domain.Posts.PostComment;

namespace UnitTests.Application.Posts.PostComment.Commands;

public class RemoveCommentFromPostHandlerTest : BasePostHandlerTest
{
    private readonly RemoveCommentFromPostHandler _handler;

    public RemoveCommentFromPostHandlerTest()
    {
        _handler = new RemoveCommentFromPostHandler(PostRepositoryMock.Object, CacheServiceMock.Object);
    }
    
    private void VerifyCommentDeleted(Post post, DateTime lastInteraction, bool success = true)
    {
        if (!success)
        {
            CommentRepositoryMock.Verify(x => x.Delete(It.IsAny<XPostComment>()), Times.Never);
            PostRepositoryMock.Verify(x => x.Update(post), Times.Never);
            PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
            CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
            Assert.Equal(lastInteraction, post.LastInteraction);
            return;
        }
        
        CommentRepositoryMock.Verify(x => x.Delete(It.IsAny<XPostComment>()), Times.Once);
        PostRepositoryMock.Verify(x => x.Update(post), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        CacheServiceMock.VerifyCacheRemove($"post-comments-{post.Id}");
        Assert.NotEqual(lastInteraction, post.LastInteraction);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldDeleteCommentAndUpdatePost()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var comment = TestDataFactory.CreateComment(post, user);
        
        PostRepositoryMock.SetupPost(post);
        CommentRepositoryMock.SetupComment(comment);
        PostRepositoryMock.SetupSaveChanges();

        var command = new RemoveCommentFromPostCommand(post.Id.ToString(), comment.Id.ToString(), user.Id.ToString());
        
        var previousLastInteraction = post.LastInteraction;

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        VerifyCommentDeleted(post, previousLastInteraction);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var commentId = Guid.NewGuid();
        
        PostRepositoryMock.SetupPost(null);
        
        var command = new RemoveCommentFromPostCommand(post.Id.ToString(), commentId.ToString(), user.Id.ToString());
        
        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        PostRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        CommentRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        
        VerifyCommentDeleted(post, previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenCommentDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var commentId = Guid.NewGuid();

        UserRepositoryMock.SetupUser(user);
        PostRepositoryMock.SetupPost(post);
        CommentRepositoryMock.SetupComment(null);

        var command = new RemoveCommentFromPostCommand(post.Id.ToString(), commentId.ToString(), user.Id.ToString());
        
        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        PostRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        CommentRepositoryMock.Verify(x => x.GetByIdAsync(commentId), Times.Once);
        
        VerifyCommentDeleted(post, previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotOwnTheComment_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var comment = TestDataFactory.CreateComment(post, user);
        var otherPost = TestDataFactory.CreatePost(user);
        
        UserRepositoryMock.SetupUser(user);
        PostRepositoryMock.SetupPost(post);
        PostRepositoryMock.SetupPost(otherPost);
        CommentRepositoryMock.SetupComment(comment);
        
        var previousLastInteraction = post.LastInteraction;

        var command = new RemoveCommentFromPostCommand(otherPost.Id.ToString(), comment.Id.ToString(), user.Id.ToString());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        PostRepositoryMock.Verify(x => x.GetByIdAsync(otherPost.Id), Times.Once);
        CommentRepositoryMock.Verify(x => x.GetByIdAsync(comment.Id), Times.Once);
        
        VerifyCommentDeleted(post, previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotOwnTheComment_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var otherUser = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var comment = TestDataFactory.CreateComment(post, user);
        
        UserRepositoryMock.SetupUser(user);
        UserRepositoryMock.SetupUser(otherUser);
        PostRepositoryMock.SetupPost(post);
        CommentRepositoryMock.SetupComment(comment);
        
        var previousLastInteraction = post.LastInteraction;

        var command = new RemoveCommentFromPostCommand(post.Id.ToString(), comment.Id.ToString(), otherUser.Id.ToString());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        PostRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        CommentRepositoryMock.Verify(x => x.GetByIdAsync(comment.Id), Times.Once);
        
        VerifyCommentDeleted(post, previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenCannotSaveChanges_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var comment = TestDataFactory.CreateComment(post, user);
        
        UserRepositoryMock.SetupUser(user);
        PostRepositoryMock.SetupPost(post);
        CommentRepositoryMock.SetupComment(comment);
        PostRepositoryMock.SetupSaveChanges(false);

        var command = new RemoveCommentFromPostCommand(post.Id.ToString(), comment.Id.ToString(), user.Id.ToString());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        PostRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        CommentRepositoryMock.Verify(x => x.GetByIdAsync(comment.Id), Times.Once);
        CommentRepositoryMock.Verify(x => x.Delete(It.IsAny<XPostComment>()), Times.Once);
        PostRepositoryMock.Verify(x => x.Update(post), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
    }
}