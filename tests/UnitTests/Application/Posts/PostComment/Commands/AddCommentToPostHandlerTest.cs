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
            UserEntityRepositoryMock.Object,
            PostEntityRepositoryMock.Object, 
            CommentEntityRepositoryMock.Object,
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
            CommentEntityRepositoryMock.Verify(x => x.Add(It.IsAny<XPostComment>()), Times.Never);
            PostEntityRepositoryMock.Verify(x => x.Update(_post), Times.Never);
            PostEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
            Assert.Equal(lastInteraction, _post.LastInteraction);
            return;
        }
        
        CommentEntityRepositoryMock.Verify(x => x.Add(It.IsAny<XPostComment>()), Times.Once);
        PostEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        CacheServiceMock.VerifyCacheRemove($"post-comments-{_post.Id}");
        Assert.True(_post.LastInteraction > lastInteraction);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldAddCommentAndUpdatePostAndDeleteCache()
    {
        // Arrange
        var command = new AddCommentToPostCommand(_post.Id.ToString(), _user.Id.ToString(), "Test comment");

        PostEntityRepositoryMock.SetupPost(_post);
        UserEntityRepositoryMock.SetupUser(_user);
        PostEntityRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        UserEntityRepositoryMock.Verify(x => x.GetByIdAsync(_user.Id, It.IsAny<CancellationToken>()), Times.Once);
        PostEntityRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id, It.IsAny<CancellationToken>()), Times.Once);
        
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

        UserEntityRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        
        VerifyCommentAdded(previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var notExistingUserId = Guid.NewGuid();
        var command = new AddCommentToPostCommand(_post.Id.ToString(), notExistingUserId.ToString(), "Test comment");

        UserEntityRepositoryMock.SetupUser(null);
        PostEntityRepositoryMock.SetupPost(_post);
        PostEntityRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = _post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        UserEntityRepositoryMock.Verify(x => x.GetByIdAsync(notExistingUserId, It.IsAny<CancellationToken>()), Times.Once);
        
        VerifyCommentAdded(previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var notExistingPostId = Guid.NewGuid();
        var command = new AddCommentToPostCommand(notExistingPostId.ToString(), _user.Id.ToString(), "Test comment");

        UserEntityRepositoryMock.SetupUser(_user);
        PostEntityRepositoryMock.SetupPost(null);
        PostEntityRepositoryMock.SetupSaveChanges();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        UserEntityRepositoryMock.Verify(x => x.GetByIdAsync(_user.Id, It.IsAny<CancellationToken>()), Times.Once);
        PostEntityRepositoryMock.Verify(x => x.GetByIdAsync(notExistingPostId, It.IsAny<CancellationToken>()), Times.Once);
        
        CommentEntityRepositoryMock.Verify(x => x.Add(It.IsAny<XPostComment>()), Times.Never);
        PostEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
    }

    [Fact]
    public async Task Handle_WhenCannotSaveChanges_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new AddCommentToPostCommand(_post.Id.ToString(), _user.Id.ToString(), "Test comment");
        
        PostEntityRepositoryMock.SetupPost(_post);
        UserEntityRepositoryMock.SetupUser(_user);
        PostEntityRepositoryMock.SetupSaveChanges(false);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        UserEntityRepositoryMock.Verify(x => x.GetByIdAsync(_user.Id, It.IsAny<CancellationToken>()), Times.Once);
        PostEntityRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id, It.IsAny<CancellationToken>()), Times.Once);
        PostEntityRepositoryMock.Verify(x => x.Update(_post), Times.Once);
        CommentEntityRepositoryMock.Verify(x => x.Add(It.IsAny<XPostComment>()), Times.Once);
        PostEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
    }
}