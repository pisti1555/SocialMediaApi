using Application.Contracts.Persistence.Repositories.Post;
using Application.Contracts.Services;
using Application.Requests.Posts.Root.Commands.DeletePost;
using ApplicationUnitTests.Factories;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Moq;

namespace ApplicationUnitTests.Requests.Posts.Root.Commands.DeletePost;

public class DeletePostHandlerTest
{
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly DeletePostHandler _deletePostHandler;

    private readonly Post _post;

    public DeletePostHandlerTest()
    {
        _postRepositoryMock = new Mock<IPostRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _deletePostHandler = new DeletePostHandler(_postRepositoryMock.Object, _cacheServiceMock.Object);

        _post = TestDataFactory.CreatePost();
    }

    [Fact]
    public async Task Handle_ShouldSucceed()
    {
        // Arrange
        var command = new DeletePostCommand(_post.Id.ToString());

        _postRepositoryMock.Setup(x => x.GetByIdAsync(_post.Id)).ReturnsAsync(_post);
        _postRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        await _deletePostHandler.Handle(command, CancellationToken.None);

        // Assert
        _postRepositoryMock.Verify(x => x.Delete(_post), Times.Once);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveAsync($"post-{_post.Id.ToString()}", CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_UnparsablePostId()
    {
        // Arrange
        var command = new DeletePostCommand("invalid-guid");

        _postRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(_post);
        _postRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _deletePostHandler.Handle(command, CancellationToken.None));
        _postRepositoryMock.Verify(x => x.Delete(_post), Times.Never);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        _cacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_PostNotFound()
    {
        // Arrange
        var command = new DeletePostCommand(_post.Id.ToString());

        _postRepositoryMock.Setup(x => x.GetByIdAsync(_post.Id)).ReturnsAsync((Post?)null);
        _postRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _deletePostHandler.Handle(command, CancellationToken.None));
        _postRepositoryMock.Verify(x => x.Delete(_post), Times.Never);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        _cacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), CancellationToken.None), Times.Never);
    }
}