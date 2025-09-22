using Application.Requests.Posts.PostComment.Commands.RemoveCommentFromPost;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Domain.Users;
using Moq;
using UnitTests.Application.Posts.Common;
using UnitTests.Extensions;
using UnitTests.Factories;
using XPostComment = Domain.Posts.PostComment;

namespace UnitTests.Application.Posts.PostComment.Commands;

public class RemoveCommentFromPostHandlerTest : BasePostHandlerTest
{
    private readonly RemoveCommentFromPostHandler _handler;
    
    private readonly AppUser _user;
    private readonly Post _post;

    public RemoveCommentFromPostHandlerTest()
    {
        _handler = new RemoveCommentFromPostHandler(PostRepositoryMock.Object, CacheServiceMock.Object);
        
        _user = TestDataFactory.CreateUser();
        _post = TestDataFactory.CreatePost(_user);
    }
    
    private void VerifyCommentDeleted(DateTime lastInteraction, bool success = true)
    {
        if (!success)
        {
            CommentRepositoryMock.Verify(x => x.Delete(It.IsAny<XPostComment>()), Times.Never);
            PostRepositoryMock.Verify(x => x.Update(_post), Times.Never);
            PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
            CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
            Assert.Equal(lastInteraction, _post.LastInteraction);
            return;
        }
        
        CommentRepositoryMock.Verify(x => x.Delete(It.IsAny<XPostComment>()), Times.Once);
        PostRepositoryMock.Verify(x => x.Update(_post), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        CacheServiceMock.VerifyCacheRemove($"post-comments-{_post.Id}");
        Assert.True(_post.LastInteraction > lastInteraction);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldDeleteCommentAndUpdatePost()
    {
        // Arrange
        var comment = TestDataFactory.CreateComment(_post, _user);
        
        PostRepositoryMock.SetupPost(_post);
        CommentRepositoryMock.SetupComment(comment);
        PostRepositoryMock.SetupSaveChanges();

        var command = new RemoveCommentFromPostCommand(_post.Id.ToString(), comment.Id.ToString(), _user.Id.ToString());
        
        var previousLastInteraction = _post.LastInteraction;

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        VerifyCommentDeleted(previousLastInteraction);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        
        PostRepositoryMock.SetupPost(null);
        
        var command = new RemoveCommentFromPostCommand(_post.Id.ToString(), commentId.ToString(), _user.Id.ToString());
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        PostRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id), Times.Once);
        CommentRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        
        VerifyCommentDeleted(previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenCommentDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var commentId = Guid.NewGuid();

        UserRepositoryMock.SetupUser(_user);
        PostRepositoryMock.SetupPost(_post);
        CommentRepositoryMock.SetupComment(null);

        var command = new RemoveCommentFromPostCommand(_post.Id.ToString(), commentId.ToString(), _user.Id.ToString());
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        PostRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id), Times.Once);
        CommentRepositoryMock.Verify(x => x.GetByIdAsync(commentId), Times.Once);
        
        VerifyCommentDeleted(previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotOwnTheComment_ShouldThrowBadRequestException()
    {
        // Arrange
        var comment = TestDataFactory.CreateComment(_post, _user);
        var otherPost = TestDataFactory.CreatePost(_user);
        
        UserRepositoryMock.SetupUser(_user);
        PostRepositoryMock.SetupPost(_post);
        PostRepositoryMock.SetupPost(otherPost);
        CommentRepositoryMock.SetupComment(comment);
        
        var previousLastInteraction = _post.LastInteraction;

        var command = new RemoveCommentFromPostCommand(otherPost.Id.ToString(), comment.Id.ToString(), _user.Id.ToString());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        PostRepositoryMock.Verify(x => x.GetByIdAsync(otherPost.Id), Times.Once);
        CommentRepositoryMock.Verify(x => x.GetByIdAsync(comment.Id), Times.Once);
        
        VerifyCommentDeleted(previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotOwnTheComment_ShouldThrowBadRequestException()
    {
        // Arrange
        var otherUser = TestDataFactory.CreateUser();
        var comment = TestDataFactory.CreateComment(_post, _user);
        
        UserRepositoryMock.SetupUser(_user);
        UserRepositoryMock.SetupUser(otherUser);
        PostRepositoryMock.SetupPost(_post);
        CommentRepositoryMock.SetupComment(comment);
        
        var previousLastInteraction = _post.LastInteraction;

        var command = new RemoveCommentFromPostCommand(_post.Id.ToString(), comment.Id.ToString(), otherUser.Id.ToString());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        PostRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id), Times.Once);
        CommentRepositoryMock.Verify(x => x.GetByIdAsync(comment.Id), Times.Once);
        
        VerifyCommentDeleted(previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenCannotSaveChanges_ShouldThrowBadRequestException()
    {
        // Arrange
        var comment = TestDataFactory.CreateComment(_post, _user);
        
        UserRepositoryMock.SetupUser(_user);
        PostRepositoryMock.SetupPost(_post);
        CommentRepositoryMock.SetupComment(comment);
        PostRepositoryMock.SetupSaveChanges(false);

        var command = new RemoveCommentFromPostCommand(_post.Id.ToString(), comment.Id.ToString(), _user.Id.ToString());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        PostRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id), Times.Once);
        CommentRepositoryMock.Verify(x => x.GetByIdAsync(comment.Id), Times.Once);
        CommentRepositoryMock.Verify(x => x.Delete(It.IsAny<XPostComment>()), Times.Once);
        PostRepositoryMock.Verify(x => x.Update(_post), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
    }
}