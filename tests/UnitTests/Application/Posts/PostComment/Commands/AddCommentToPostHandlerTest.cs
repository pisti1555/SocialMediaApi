using Application.Requests.Posts.PostComment.Commands.AddCommentToPost;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Domain.Users;
using Moq;
using UnitTests.Application.Posts.Common;
using UnitTests.Extensions;
using UnitTests.Factories;
using XPostComment = Domain.Posts.PostComment;

namespace UnitTests.Application.Posts.PostComment.Commands;

public class AddCommentToPostHandlerTest : BasePostHandlerTest
{
    private readonly AddCommentToPostHandler _handler;
    
    private readonly AppUser _user;
    private readonly Post _post;

    public AddCommentToPostHandlerTest()
    {
        _handler = new AddCommentToPostHandler(
            UserRepositoryMock.Object,
            PostRepositoryMock.Object, 
            CommentRepositoryMock.Object,
            CacheServiceMock.Object, 
            Mapper
        );
        
        _user = TestDataFactory.CreateUser();
        _post = TestDataFactory.CreatePost(_user);
    }
    
    private void VerifyCommentAdded(DateTime lastInteraction, bool success = true)
    {
        if (!success)
        {
            CommentRepositoryMock.Verify(x => x.Add(It.IsAny<XPostComment>()), Times.Never);
            PostRepositoryMock.Verify(x => x.Update(_post), Times.Never);
            PostRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
            Assert.Equal(lastInteraction, _post.LastInteraction);
            return;
        }
        
        CommentRepositoryMock.Verify(x => x.Add(It.IsAny<XPostComment>()), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        CacheServiceMock.VerifyCacheRemove($"post-comments-{_post.Id}");
        Assert.True(_post.LastInteraction > lastInteraction);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldAddCommentAndUpdatePostAndDeleteCache()
    {
        // Arrange
        var command = new AddCommentToPostCommand(_post.Id.ToString(), _user.Id.ToString(), "Test comment");

        PostRepositoryMock.SetupPost(_post, Mapper);
        UserRepositoryMock.SetupUser(_user, Mapper);
        PostRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        UserRepositoryMock.Verify(x => x.GetEntityByIdAsync(_user.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetEntityByIdAsync(_post.Id), Times.Once);
        
        VerifyCommentAdded(previousLastInteraction);
    }

    [Fact]
    public async Task Handle_WhenUnparsableUserId_ShouldThrowBadRequestException()
    {
        // Arrange
        const string invalidUserId = "invalid-user-guid";
        var command = new AddCommentToPostCommand(_post.Id.ToString(), invalidUserId, "Test comment");

        var previousLastInteraction = _post.LastInteraction;
        
        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        UserRepositoryMock.Verify(x => x.GetEntityByIdAsync(It.IsAny<Guid>()), Times.Never);
        
        VerifyCommentAdded(previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var notExistingUserId = Guid.NewGuid();
        var command = new AddCommentToPostCommand(_post.Id.ToString(), notExistingUserId.ToString(), "Test comment");

        UserRepositoryMock.SetupUser(null, Mapper);
        PostRepositoryMock.SetupPost(_post, Mapper);
        PostRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        UserRepositoryMock.Verify(x => x.GetEntityByIdAsync(notExistingUserId), Times.Once);
        
        VerifyCommentAdded(previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var notExistingPostId = Guid.NewGuid();
        var command = new AddCommentToPostCommand(notExistingPostId.ToString(), _user.Id.ToString(), "Test comment");

        UserRepositoryMock.SetupUser(_user, Mapper);
        PostRepositoryMock.SetupPost(null, Mapper);
        PostRepositoryMock.SetupSaveChanges();

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        UserRepositoryMock.Verify(x => x.GetEntityByIdAsync(_user.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetEntityByIdAsync(notExistingPostId), Times.Once);
        
        CommentRepositoryMock.Verify(x => x.Add(It.IsAny<XPostComment>()), Times.Never);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
    }

    [Fact]
    public async Task Handle_WhenCannotSaveChanges_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new AddCommentToPostCommand(_post.Id.ToString(), _user.Id.ToString(), "Test comment");
        
        PostRepositoryMock.SetupPost(_post, Mapper);
        UserRepositoryMock.SetupUser(_user, Mapper);
        PostRepositoryMock.SetupSaveChanges(false);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.GetEntityByIdAsync(_user.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetEntityByIdAsync(_post.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.Update(_post), Times.Once);
        CommentRepositoryMock.Verify(x => x.Add(It.IsAny<XPostComment>()), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
    }
}