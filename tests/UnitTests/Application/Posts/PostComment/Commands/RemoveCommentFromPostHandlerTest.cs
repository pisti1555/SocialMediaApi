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
        _handler = new RemoveCommentFromPostHandler(
            PostEntityRepositoryMock.Object, 
            CommentEntityRepositoryMock.Object, 
            CacheServiceMock.Object
        );
        
        _user = TestDataFactory.CreateUser();
        _post = TestDataFactory.CreatePost(_user);
    }
    
    private void VerifyCommentDeleted(DateTime lastInteraction, bool success = true)
    {
        if (!success)
        {
            CommentEntityRepositoryMock.Verify(x => x.Delete(It.IsAny<XPostComment>()), Times.Never);
            PostEntityRepositoryMock.Verify(x => x.Update(_post), Times.Never);
            PostEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
            Assert.Equal(lastInteraction, _post.LastInteraction);
            return;
        }
        
        CommentEntityRepositoryMock.Verify(x => x.Delete(It.IsAny<XPostComment>()), Times.Once);
        PostEntityRepositoryMock.Verify(x => x.Update(_post), Times.Once);
        PostEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        CacheServiceMock.VerifyCacheRemove($"post-comments-{_post.Id}");
        Assert.True(_post.LastInteraction > lastInteraction);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldDeleteCommentAndUpdatePost()
    {
        // Arrange
        var comment = TestDataFactory.CreateComment(_post, _user);
        
        PostEntityRepositoryMock.SetupPost(_post);
        CommentEntityRepositoryMock.SetupComment(comment);
        PostEntityRepositoryMock.SetupSaveChanges();

        var command = new RemoveCommentFromPostCommand(_post.Id.ToString(), comment.Id.ToString(), _user.Id.ToString());
        
        var previousLastInteraction = _post.LastInteraction;

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        VerifyCommentDeleted(previousLastInteraction);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        
        PostEntityRepositoryMock.SetupPost(null);
        
        var command = new RemoveCommentFromPostCommand(_post.Id.ToString(), commentId.ToString(), _user.Id.ToString());
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        
        PostEntityRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id, It.IsAny<CancellationToken>()), Times.Once);
        CommentEntityRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        
        VerifyCommentDeleted(previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenCommentDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        
        PostEntityRepositoryMock.SetupPost(_post);
        CommentEntityRepositoryMock.SetupComment(null);

        var command = new RemoveCommentFromPostCommand(_post.Id.ToString(), commentId.ToString(), _user.Id.ToString());
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        
        PostEntityRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id, It.IsAny<CancellationToken>()), Times.Once);
        CommentEntityRepositoryMock.Verify(x => x.GetByIdAsync(commentId, It.IsAny<CancellationToken>()), Times.Once);
        
        VerifyCommentDeleted(previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotOwnTheComment_ShouldThrowBadRequestException()
    {
        // Arrange
        var comment = TestDataFactory.CreateComment(_post, _user);
        var otherPost = TestDataFactory.CreatePost(_user);
        
        PostEntityRepositoryMock.SetupPost(_post);
        PostEntityRepositoryMock.SetupPost(otherPost);
        CommentEntityRepositoryMock.SetupComment(comment);
        
        var previousLastInteraction = _post.LastInteraction;

        var command = new RemoveCommentFromPostCommand(otherPost.Id.ToString(), comment.Id.ToString(), _user.Id.ToString());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        PostEntityRepositoryMock.Verify(x => x.GetByIdAsync(otherPost.Id, It.IsAny<CancellationToken>()), Times.Once);
        CommentEntityRepositoryMock.Verify(x => x.GetByIdAsync(comment.Id, It.IsAny<CancellationToken>()), Times.Once);
        
        VerifyCommentDeleted(previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotOwnTheComment_ShouldThrowBadRequestException()
    {
        // Arrange
        var otherUser = TestDataFactory.CreateUser();
        var comment = TestDataFactory.CreateComment(_post, _user);
        
        PostEntityRepositoryMock.SetupPost(_post);
        CommentEntityRepositoryMock.SetupComment(comment);
        
        var previousLastInteraction = _post.LastInteraction;

        var command = new RemoveCommentFromPostCommand(_post.Id.ToString(), comment.Id.ToString(), otherUser.Id.ToString());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        PostEntityRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id, It.IsAny<CancellationToken>()), Times.Once);
        CommentEntityRepositoryMock.Verify(x => x.GetByIdAsync(comment.Id, It.IsAny<CancellationToken>()), Times.Once);
        
        VerifyCommentDeleted(previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenCannotSaveChanges_ShouldThrowBadRequestException()
    {
        // Arrange
        var comment = TestDataFactory.CreateComment(_post, _user);
        
        PostEntityRepositoryMock.SetupPost(_post);
        CommentEntityRepositoryMock.SetupComment(comment);
        PostEntityRepositoryMock.SetupSaveChanges(false);

        var command = new RemoveCommentFromPostCommand(_post.Id.ToString(), comment.Id.ToString(), _user.Id.ToString());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        PostEntityRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id, It.IsAny<CancellationToken>()), Times.Once);
        CommentEntityRepositoryMock.Verify(x => x.GetByIdAsync(comment.Id, It.IsAny<CancellationToken>()), Times.Once);
        CommentEntityRepositoryMock.Verify(x => x.Delete(It.IsAny<XPostComment>()), Times.Once);
        PostEntityRepositoryMock.Verify(x => x.Update(_post), Times.Once);
        PostEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
    }
}