using Application.Common.Interfaces.Persistence.Repositories.AppUser;
using Application.Common.Interfaces.Persistence.Repositories.Post;
using Application.Requests.Posts.Root.Commands.CreatePost;
using ApplicationUnitTests.Common;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Domain.Users;
using Moq;

namespace ApplicationUnitTests.Requests.Posts.Root.Commands.CreatePost;

public class CreatePostHandlerTest
{
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly Mock<IAppUserRepository> _userRepositoryMock;
    private readonly CreatePostHandler _createPostHandler;

    public CreatePostHandlerTest()
    {
        _postRepositoryMock = new Mock<IPostRepository>();
        _userRepositoryMock = new Mock<IAppUserRepository>();
        
        _createPostHandler = new CreatePostHandler(_postRepositoryMock.Object, _userRepositoryMock.Object, TestMapperSetup.SetupMapper());
    }

    [Fact]
    public async Task Handle_ShouldSucceed()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var command = new CreatePostCommand("Test post text", user.Id.ToString());
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _postRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        await _createPostHandler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(x => x.GetByIdAsync(user.Id), Times.Once);
        _postRepositoryMock.Verify(x => x.Add(It.IsAny<Post>()), Times.Once);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_TextTooLong()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var command = new CreatePostCommand(new string('a', 20001), user.Id.ToString());
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _postRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createPostHandler.Handle(command, CancellationToken.None));
        _userRepositoryMock.Verify(x => x.GetByIdAsync(user.Id), Times.Once);
        _postRepositoryMock.Verify(x => x.Add(It.IsAny<Post>()), Times.Never);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_UnparsableUserId()
    {
        // Arrange
        const string invalidUserId = "invalid-guid";
        var command = new CreatePostCommand("Test post text", invalidUserId);
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((AppUser?)null);
        _postRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createPostHandler.Handle(command, CancellationToken.None));
        _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _postRepositoryMock.Verify(x => x.Add(It.IsAny<Post>()), Times.Never);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_UserNotFound()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var command = new CreatePostCommand("Test post text", guid.ToString());
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(guid)).ReturnsAsync((AppUser?)null);
        _postRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createPostHandler.Handle(command, CancellationToken.None));
        _userRepositoryMock.Verify(x => x.GetByIdAsync(guid), Times.Once);
        _postRepositoryMock.Verify(x => x.Add(It.IsAny<Post>()), Times.Never);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }
}