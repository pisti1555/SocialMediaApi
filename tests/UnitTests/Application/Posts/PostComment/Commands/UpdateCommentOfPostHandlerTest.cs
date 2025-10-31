using Application.Requests.Posts.PostComment.Commands.UpdateCommentOfPost;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Domain.Users;
using Moq;
using UnitTests.Application.Posts.Common;
using UnitTests.Extensions;
using UnitTests.Factories;
using XPostComment = Domain.Posts.PostComment;

namespace UnitTests.Application.Posts.PostComment.Commands;

public class UpdateCommentOfPostHandlerTest : BasePostHandlerTest
{
     private readonly UpdateCommentOfPostHandler _updateCommentOfPostHandler;
    
    private readonly AppUser _user;
    private readonly Post _post;
    private readonly XPostComment _comment;
    
    public UpdateCommentOfPostHandlerTest()
    {
        _updateCommentOfPostHandler = new UpdateCommentOfPostHandler(
            UserEntityRepositoryMock.Object, 
            PostEntityRepositoryMock.Object, 
            CommentEntityRepositoryMock.Object,
            CacheServiceMock.Object,
            Mapper
        );
        
        _user = TestDataFactory.CreateUser();
        _post = TestDataFactory.CreatePost(_user);
        _comment = TestDataFactory.CreateComment(_post, _user);
    }
    
    private void VerifyCommentUpdated(DateTime lastInteraction, bool success = true)
    {
        if (!success)
        {
            CommentEntityRepositoryMock.Verify(x => x.Update(It.IsAny<XPostComment>()), Times.Never);
            PostEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
            Assert.Equal(lastInteraction, _post.LastInteraction);
            return;
        }
        
        CommentEntityRepositoryMock.Verify(x => x.Update(It.IsAny<XPostComment>()), Times.Once);
        PostEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        CacheServiceMock.VerifyCacheRemove(It.IsAny<string>());
        Assert.True(_post.LastInteraction > lastInteraction);
    }
    
    [Fact]
    public async Task Handle_WhenValidRequest_ShouldUpdateCommentAndRemoveCachedCommentsOfPost()
    {
        // Arrange
        var command = new UpdateCommentOfPostCommand(_post.Id.ToString(), _comment.Id.ToString(), _user.Id.ToString(), "Updated comment text");
        
        UserEntityRepositoryMock.SetupUser(_user);
        PostEntityRepositoryMock.SetupPost(_post);
        CommentEntityRepositoryMock.SetupComment(_comment);
        PostEntityRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act
        await _updateCommentOfPostHandler.Handle(command, CancellationToken.None);

        // Assert
        VerifyCommentUpdated(previousLastInteraction);
        Assert.True(_post.LastInteraction > previousLastInteraction);
    }
    
    [Fact]
    public async Task Handle_WhenUserDoesNotOwnComment_ShouldThrowBadRequestException()
    {
        // Arrange
        var otherUser = TestDataFactory.CreateUser();
        
        var command = new UpdateCommentOfPostCommand(_post.Id.ToString(), _comment.Id.ToString(), otherUser.Id.ToString(), "Updated post text");
        
        UserEntityRepositoryMock.SetupUser(_user);
        UserEntityRepositoryMock.SetupUser(otherUser);
        PostEntityRepositoryMock.SetupPost(_post);
        CommentEntityRepositoryMock.SetupComment(_comment);
        PostEntityRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _updateCommentOfPostHandler.Handle(command, CancellationToken.None));
        
        VerifyCommentUpdated(previousLastInteraction, success: false);
        Assert.Equal(previousLastInteraction, _post.LastInteraction);
    }
    
    [Fact]
    public async Task Handle_WhenPostDoesNotOwnComment_ShouldThrowBadRequestException()
    {
        // Arrange
        var otherPost = TestDataFactory.CreatePost();
        
        var command = new UpdateCommentOfPostCommand(otherPost.Id.ToString(), _comment.Id.ToString(), _user.Id.ToString(), "Updated post text");
        
        UserEntityRepositoryMock.SetupUser(_user);
        PostEntityRepositoryMock.SetupPost(_post);
        PostEntityRepositoryMock.SetupPost(otherPost);
        CommentEntityRepositoryMock.SetupComment(_comment);
        PostEntityRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _updateCommentOfPostHandler.Handle(command, CancellationToken.None));
        
        VerifyCommentUpdated(previousLastInteraction, success: false);
        Assert.Equal(previousLastInteraction, _post.LastInteraction);
    }
    
    [Fact]
    public async Task Handle_WhenTextTooLong_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new UpdateCommentOfPostCommand(_post.Id.ToString(), _comment.Id.ToString(), _user.Id.ToString(), new string('a', 1001));
        
        UserEntityRepositoryMock.SetupUser(_user);
        PostEntityRepositoryMock.SetupPost(_post);
        CommentEntityRepositoryMock.SetupComment(_comment);
        PostEntityRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;
        
        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _updateCommentOfPostHandler.Handle(command, CancellationToken.None));
        
        VerifyCommentUpdated(previousLastInteraction, success: false);
        Assert.Equal(previousLastInteraction, _post.LastInteraction);
    }
    
    [Fact]
    public async Task Handle_WhenUnparsableUserId_ShouldThrowBadRequestException()
    {
        // Arrange
        const string invalidUserId = "invalid-guid";
        var command = new UpdateCommentOfPostCommand(_post.Id.ToString(), _comment.Id.ToString(), invalidUserId, "Updated comment text");
        
        UserEntityRepositoryMock.SetupUser(_user);
        PostEntityRepositoryMock.SetupPost(_post);
        CommentEntityRepositoryMock.SetupComment(_comment);
        PostEntityRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _updateCommentOfPostHandler.Handle(command, CancellationToken.None));
        
        UserEntityRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        PostEntityRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id, It.IsAny<CancellationToken>()), Times.Never);
        CommentEntityRepositoryMock.Verify(x => x.GetByIdAsync(_comment.Id, It.IsAny<CancellationToken>()), Times.Never);
        
        VerifyCommentUpdated(previousLastInteraction, success: false);
        Assert.Equal(previousLastInteraction, _post.LastInteraction);
    }
    
    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowBadRequestException()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var command = new UpdateCommentOfPostCommand(_post.Id.ToString(), _comment.Id.ToString(), guid.ToString(), "Updated comment text");
        
        UserEntityRepositoryMock.SetupUser(null);
        PostEntityRepositoryMock.SetupPost(_post);
        CommentEntityRepositoryMock.SetupComment(_comment);
        PostEntityRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _updateCommentOfPostHandler.Handle(command, CancellationToken.None));
        
        VerifyCommentUpdated(previousLastInteraction, success: false);
        Assert.Equal(previousLastInteraction, _post.LastInteraction);
    }
    
    [Fact]
    public async Task Handle_WhenPostNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var command = new UpdateCommentOfPostCommand(guid.ToString(), _comment.Id.ToString(), _user.Id.ToString(), "Updated comment text");
        
        UserEntityRepositoryMock.SetupUser(_user);
        PostEntityRepositoryMock.SetupPost(null);
        CommentEntityRepositoryMock.SetupComment(_comment);
        PostEntityRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _updateCommentOfPostHandler.Handle(command, CancellationToken.None));
        
        VerifyCommentUpdated(previousLastInteraction, success: false);
        Assert.Equal(previousLastInteraction, _post.LastInteraction);
    }
    
    [Fact]
    public async Task Handle_WhenCommentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var command = new UpdateCommentOfPostCommand(_post.Id.ToString(), guid.ToString(), _user.Id.ToString(), "Updated comment text");
        
        UserEntityRepositoryMock.SetupUser(_user);
        PostEntityRepositoryMock.SetupPost(_post);
        CommentEntityRepositoryMock.SetupComment(null);
        PostEntityRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _updateCommentOfPostHandler.Handle(command, CancellationToken.None));
        
        VerifyCommentUpdated(previousLastInteraction, success: false);
        Assert.Equal(previousLastInteraction, _post.LastInteraction);
    }
}