using Application.Contracts.Persistence.Repositories.Post;
using Application.Requests.Posts.Root.Commands.DeletePost;
using ApplicationUnitTests.Common;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Moq;

namespace ApplicationUnitTests.Requests.Posts.Root.Commands.DeletePost;

public class DeletePostHandlerTest
{
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly DeletePostHandler _deletePostHandler;

    public DeletePostHandlerTest()
    {
        _postRepositoryMock = new Mock<IPostRepository>();
        _deletePostHandler = new DeletePostHandler(_postRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSucceed()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);
        var command = new DeletePostCommand(post.Id.ToString());

        _postRepositoryMock.Setup(x => x.GetByIdAsync(post.Id)).ReturnsAsync(post);
        _postRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        await _deletePostHandler.Handle(command, CancellationToken.None);

        // Assert
        _postRepositoryMock.Verify(x => x.Delete(post), Times.Once);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_UnparsablePostId()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);
        var command = new DeletePostCommand("invalid-guid");

        _postRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(post);
        _postRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

        // Act & Assert
        _postRepositoryMock.Verify(x => x.Delete(post), Times.Never);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        await Assert.ThrowsAsync<BadRequestException>(() => _deletePostHandler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_PostNotFound()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);
        var command = new DeletePostCommand(post.Id.ToString());

        _postRepositoryMock.Setup(x => x.GetByIdAsync(post.Id)).ReturnsAsync((Post?)null);
        _postRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

        // Act & Assert
        _postRepositoryMock.Verify(x => x.Delete(post), Times.Never);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        await Assert.ThrowsAsync<BadRequestException>(() => _deletePostHandler.Handle(command, CancellationToken.None));
    }
}