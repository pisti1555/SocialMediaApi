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
            UserRepositoryMock.Object, 
            PostRepositoryMock.Object, 
            CommentRepositoryMock.Object,
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
            CommentRepositoryMock.Verify(x => x.Update(It.IsAny<XPostComment>()), Times.Never);
            PostRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
            Assert.Equal(lastInteraction, _post.LastInteraction);
            return;
        }
        
        CommentRepositoryMock.Verify(x => x.Update(It.IsAny<XPostComment>()), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        CacheServiceMock.VerifyCacheRemove(It.IsAny<string>());
        Assert.True(_post.LastInteraction > lastInteraction);
    }
    
    [Fact]
    public async Task Handle_WhenValidRequest_ShouldUpdateCommentAndRemoveCachedCommentsOfPost()
    {
        // Arrange
        var command = new UpdateCommentOfPostCommand(_post.Id.ToString(), _comment.Id.ToString(), _user.Id.ToString(), "Updated comment text");
        
        UserRepositoryMock.SetupUser(_user, Mapper);
        PostRepositoryMock.SetupPost(_post, Mapper);
        CommentRepositoryMock.SetupComment(_comment, Mapper);
        PostRepositoryMock.SetupSaveChanges();
        
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
        
        UserRepositoryMock.SetupUser(_user, Mapper);
        UserRepositoryMock.SetupUser(otherUser, Mapper);
        PostRepositoryMock.SetupPost(_post, Mapper);
        CommentRepositoryMock.SetupComment(_comment, Mapper);
        PostRepositoryMock.SetupSaveChanges();
        
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
        
        UserRepositoryMock.SetupUser(_user, Mapper);
        PostRepositoryMock.SetupPost(_post, Mapper);
        PostRepositoryMock.SetupPost(otherPost, Mapper);
        CommentRepositoryMock.SetupComment(_comment, Mapper);
        PostRepositoryMock.SetupSaveChanges();
        
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
        
        UserRepositoryMock.SetupUser(_user, Mapper);
        PostRepositoryMock.SetupPost(_post, Mapper);
        CommentRepositoryMock.SetupComment(_comment, Mapper);
        PostRepositoryMock.SetupSaveChanges();
        
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
        
        UserRepositoryMock.SetupUser(_user, Mapper);
        PostRepositoryMock.SetupPost(_post, Mapper);
        CommentRepositoryMock.SetupComment(_comment, Mapper);
        PostRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _updateCommentOfPostHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.GetEntityByIdAsync(It.IsAny<Guid>()), Times.Never);
        PostRepositoryMock.Verify(x => x.GetEntityByIdAsync(_post.Id), Times.Never);
        CommentRepositoryMock.Verify(x => x.GetEntityByIdAsync(_comment.Id), Times.Never);
        
        VerifyCommentUpdated(previousLastInteraction, success: false);
        Assert.Equal(previousLastInteraction, _post.LastInteraction);
    }
    
    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowBadRequestException()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var command = new UpdateCommentOfPostCommand(_post.Id.ToString(), _comment.Id.ToString(), guid.ToString(), "Updated comment text");
        
        UserRepositoryMock.SetupUser(null, Mapper);
        PostRepositoryMock.SetupPost(_post, Mapper);
        CommentRepositoryMock.SetupComment(_comment, Mapper);
        PostRepositoryMock.SetupSaveChanges();
        
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
        
        UserRepositoryMock.SetupUser(_user, Mapper);
        PostRepositoryMock.SetupPost(null, Mapper);
        CommentRepositoryMock.SetupComment(_comment, Mapper);
        PostRepositoryMock.SetupSaveChanges();
        
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
        
        UserRepositoryMock.SetupUser(_user, Mapper);
        PostRepositoryMock.SetupPost(_post, Mapper);
        CommentRepositoryMock.SetupComment(null, Mapper);
        PostRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _updateCommentOfPostHandler.Handle(command, CancellationToken.None));
        
        VerifyCommentUpdated(previousLastInteraction, success: false);
        Assert.Equal(previousLastInteraction, _post.LastInteraction);
    }
}