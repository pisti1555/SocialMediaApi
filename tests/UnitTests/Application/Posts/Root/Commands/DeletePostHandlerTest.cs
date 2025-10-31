using System.Linq.Expressions;
using Application.Requests.Posts.Root.Commands.DeletePost;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Domain.Users;
using Moq;
using UnitTests.Application.Posts.Common;
using UnitTests.Extensions;
using UnitTests.Factories;

namespace UnitTests.Application.Posts.Root.Commands;

public class DeletePostHandlerTest : BasePostHandlerTest
{
    private readonly DeletePostHandler _deletePostHandler;

    private readonly AppUser _user;
    private readonly Post _post;

    public DeletePostHandlerTest()
    {
        _deletePostHandler = new DeletePostHandler(PostEntityRepositoryMock.Object, CacheServiceMock.Object);

        _user = TestDataFactory.CreateUser();
        _post = TestDataFactory.CreatePost(_user);
    }

    private void AssertPostDeleted(Post post, bool success = true)
    {
        PostEntityRepositoryMock.Verify(x => x.Delete(post), success ? Times.Once : Times.Never);
        PostEntityRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), success ? Times.Once : Times.Never);
        CacheServiceMock.VerifyCacheRemove($"post-{post.Id}", success);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldDeletePostAndDeleteCache()
    {
        // Arrange
        var command = new DeletePostCommand(_post.Id.ToString(), _user.Id.ToString());

        PostEntityRepositoryMock.SetupPost(_post);
        PostEntityRepositoryMock.SetupPostExistsByAnyFilters(true);
        PostEntityRepositoryMock.SetupSaveChanges();

        // Act
        await _deletePostHandler.Handle(command, CancellationToken.None);

        // Assert
        PostEntityRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<Post, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        AssertPostDeleted(_post);
    }

    [Fact]
    public async Task Handle_WhenUnparsablePostId_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new DeletePostCommand("invalid-guid", _user.Id.ToString());

        PostEntityRepositoryMock.SetupPost(_post);
        PostEntityRepositoryMock.SetupPostExistsByAnyFilters(true);
        PostEntityRepositoryMock.SetupSaveChanges();

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _deletePostHandler.Handle(command, CancellationToken.None));
        
        PostEntityRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<Post, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
        AssertPostDeleted(_post, false);
    }
    
    [Fact]
    public async Task Handle_WhenUnparsableUserId_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new DeletePostCommand(_post.Id.ToString(), "invalid-guid");

        PostEntityRepositoryMock.SetupPost(_post);
        PostEntityRepositoryMock.SetupPostExistsByAnyFilters(true);
        PostEntityRepositoryMock.SetupSaveChanges();

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _deletePostHandler.Handle(command, CancellationToken.None));
        
        PostEntityRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<Post, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
        AssertPostDeleted(_post, false);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new DeletePostCommand(_post.Id.ToString(), _user.Id.ToString());

        PostEntityRepositoryMock.SetupPost(null);
        PostEntityRepositoryMock.SetupPostExistsByAnyFilters(true);
        PostEntityRepositoryMock.SetupSaveChanges();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _deletePostHandler.Handle(command, CancellationToken.None));
        
        PostEntityRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<Post, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
        AssertPostDeleted(_post, false);
    }
    
    [Fact]
    public async Task Handle_WhenUserDoesNotOwnPost_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new DeletePostCommand(_post.Id.ToString(), _user.Id.ToString());

        PostEntityRepositoryMock.SetupPost(_post);
        PostEntityRepositoryMock.SetupPostExistsByAnyFilters(false);
        PostEntityRepositoryMock.SetupSaveChanges();

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _deletePostHandler.Handle(command, CancellationToken.None));
        
        PostEntityRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<Post, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        AssertPostDeleted(_post, false);
    }
}