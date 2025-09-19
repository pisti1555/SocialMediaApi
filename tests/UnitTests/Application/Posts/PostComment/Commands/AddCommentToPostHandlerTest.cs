using Application.Requests.Posts.PostComment.Commands.AddCommentToPost;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Moq;
using UnitTests.Application.Posts.Common;
using UnitTests.Extensions;
using UnitTests.Factories;
using XPostComment = Domain.Posts.PostComment;

namespace UnitTests.Application.Posts.PostComment.Commands;

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
    
    private void VerifyCommentAdded(Post post, DateTime lastInteraction, bool success = true)
    {
        if (!success)
        {
            CommentRepositoryMock.Verify(x => x.Add(It.IsAny<XPostComment>()), Times.Never);
            PostRepositoryMock.Verify(x => x.Update(post), Times.Never);
            PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
            CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
            Assert.Equal(lastInteraction, post.LastInteraction);
            return;
        }
        
        CommentRepositoryMock.Verify(x => x.Add(It.IsAny<XPostComment>()), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        CacheServiceMock.VerifyCacheRemove($"post-comments-{post.Id}");
        Assert.NotEqual(lastInteraction, post.LastInteraction);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldAddCommentAndUpdatePostAndDeleteCache()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var command = new AddCommentToPostCommand(post.Id.ToString(), user.Id.ToString(), "Test comment");

        PostRepositoryMock.SetupPost(post);
        UserRepositoryMock.SetupUser(user);
        PostRepositoryMock.SetupSaveChanges();
        
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

        var previousLastInteraction = post.LastInteraction;
        
        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        UserRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        
        VerifyCommentAdded(post, previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var post = TestDataFactory.CreatePost();
        var notExistingUserId = Guid.NewGuid();
        var command = new AddCommentToPostCommand(post.Id.ToString(), notExistingUserId.ToString(), "Test comment");

        UserRepositoryMock.SetupUser(null);
        PostRepositoryMock.SetupPost(post);
        PostRepositoryMock.SetupSaveChanges();
        
        var previousLastInteraction = post.LastInteraction;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        UserRepositoryMock.Verify(x => x.GetByIdAsync(notExistingUserId), Times.Once);
        
        VerifyCommentAdded(post, previousLastInteraction, false);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var notExistingPostId = Guid.NewGuid();
        var command = new AddCommentToPostCommand(notExistingPostId.ToString(), user.Id.ToString(), "Test comment");

        UserRepositoryMock.SetupUser(user);
        PostRepositoryMock.SetupPost(null);
        PostRepositoryMock.SetupSaveChanges();

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        UserRepositoryMock.Verify(x => x.GetByIdAsync(user.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetByIdAsync(notExistingPostId), Times.Once);
        
        CommentRepositoryMock.Verify(x => x.Add(It.IsAny<XPostComment>()), Times.Never);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
    }

    [Fact]
    public async Task Handle_WhenCannotSaveChanges_ShouldThrowBadRequestException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var post = TestDataFactory.CreatePost(user);
        var command = new AddCommentToPostCommand(post.Id.ToString(), user.Id.ToString(), "Test comment");
        
        PostRepositoryMock.SetupPost(post);
        UserRepositoryMock.SetupUser(user);
        PostRepositoryMock.SetupSaveChanges(false);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.GetByIdAsync(user.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        PostRepositoryMock.Verify(x => x.Update(post), Times.Once);
        CommentRepositoryMock.Verify(x => x.Add(It.IsAny<XPostComment>()), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        CacheServiceMock.VerifyCacheRemove(It.IsAny<string>(), false);
    }
}