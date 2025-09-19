using Application.Requests.Posts.Root.Commands.DeletePost;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Moq;
using UnitTests.Application.Posts.Common;
using UnitTests.Extensions;
using UnitTests.Factories;

namespace UnitTests.Application.Posts.Root.Commands;

public class DeletePostHandlerTest : BasePostHandlerTest
{
    private readonly DeletePostHandler _deletePostHandler;

    private readonly Post _post;

    public DeletePostHandlerTest()
    {
        _deletePostHandler = new DeletePostHandler(PostRepositoryMock.Object, CacheServiceMock.Object);

        _post = TestDataFactory.CreatePost();
    }

    private void AssertPostDeleted(Post post, bool success = true)
    {
        PostRepositoryMock.Verify(x => x.Delete(post), success ? Times.Once : Times.Never);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), success ? Times.Once : Times.Never);
        CacheServiceMock.VerifyCacheRemove($"post-{post.Id}", success);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldDeletePostAndDeleteCache()
    {
        // Arrange
        var command = new DeletePostCommand(_post.Id.ToString());

        PostRepositoryMock.SetupPost(_post);
        PostRepositoryMock.SetupSaveChanges();

        // Act
        await _deletePostHandler.Handle(command, CancellationToken.None);

        // Assert
        AssertPostDeleted(_post);
    }

    [Fact]
    public async Task Handle_WhenUnparsablePostId_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new DeletePostCommand("invalid-guid");

        PostRepositoryMock.SetupPost(_post);
        PostRepositoryMock.SetupSaveChanges();

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _deletePostHandler.Handle(command, CancellationToken.None));
        
        AssertPostDeleted(_post, false);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new DeletePostCommand(_post.Id.ToString());

        PostRepositoryMock.SetupPost(null);
        PostRepositoryMock.SetupSaveChanges();

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _deletePostHandler.Handle(command, CancellationToken.None));
        
        AssertPostDeleted(_post, false);
    }
}