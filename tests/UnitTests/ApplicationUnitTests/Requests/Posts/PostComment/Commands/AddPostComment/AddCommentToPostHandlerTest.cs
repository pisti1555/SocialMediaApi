using Application.Contracts.Persistence.Repositories.AppUser;
using Application.Contracts.Persistence.Repositories.Post;
using Application.Requests.Posts.PostComment.Commands.AddCommentToPost;
using ApplicationUnitTests.Common;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Domain.Posts.Factories;
using Domain.Users;
using Moq;

namespace ApplicationUnitTests.Requests.Posts.PostComment.Commands.AddPostComment;

public class AddCommentToPostHandlerTest
{
    private readonly Mock<IAppUserRepository> _userRepositoryMock;
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly Mock<IPostCommentRepository> _commentRepositoryMock;
    private readonly AddCommentToPostHandler _handler;

    public AddCommentToPostHandlerTest()
    {
        _userRepositoryMock = new Mock<IAppUserRepository>();
        _postRepositoryMock = new Mock<IPostRepository>();
        _commentRepositoryMock = new Mock<IPostCommentRepository>();

        _postRepositoryMock.SetupGet(x => x.CommentRepository).Returns(_commentRepositoryMock.Object);

        _handler = new AddCommentToPostHandler(_postRepositoryMock.Object, _userRepositoryMock.Object, TestMapperSetup.SetupMapper());
    }

    [Fact]
    public async Task Handle_ShouldSucceed()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);
        var command = new AddCommentToPostCommand(post.Id.ToString(), user.Id.ToString(), "Test comment");

        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _postRepositoryMock.Setup(x => x.GetByIdAsync(post.Id)).ReturnsAsync(post);
        _postRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(x => x.GetByIdAsync(user.Id), Times.Once);
        _postRepositoryMock.Verify(x => x.GetByIdAsync(post.Id), Times.Once);
        _commentRepositoryMock.Verify(x => x.Add(It.IsAny<Domain.Posts.PostComment>()), Times.Once);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_UnparsableUserId()
    {
        // Arrange
        var postId = Guid.NewGuid().ToString();
        const string invalidUserId = "invalid-user-guid";
        var command = new AddCommentToPostCommand(postId, invalidUserId, "Test comment");

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _commentRepositoryMock.Verify(x => x.Add(It.IsAny<Domain.Posts.PostComment>()), Times.Never);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_UserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var command = new AddCommentToPostCommand(postId.ToString(), userId.ToString(), "Test comment");

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((AppUser?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
        _commentRepositoryMock.Verify(x => x.Add(It.IsAny<Domain.Posts.PostComment>()), Times.Never);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_PostNotFound()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var postId = Guid.NewGuid();
        var command = new AddCommentToPostCommand(postId.ToString(), user.Id.ToString(), "Test comment");

        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _postRepositoryMock.Setup(x => x.GetByIdAsync(postId)).ReturnsAsync((Post?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        _userRepositoryMock.Verify(x => x.GetByIdAsync(user.Id), Times.Once);
        _postRepositoryMock.Verify(x => x.GetByIdAsync(postId), Times.Once);
        _commentRepositoryMock.Verify(x => x.Add(It.IsAny<Domain.Posts.PostComment>()), Times.Never);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_CannotSaveChanges()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = PostFactory.Create("Test post text", user);
        var command = new AddCommentToPostCommand(post.Id.ToString(), user.Id.ToString(), "Test comment");

        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _postRepositoryMock.Setup(x => x.GetByIdAsync(post.Id)).ReturnsAsync(post);
        _postRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(command, CancellationToken.None));

        _commentRepositoryMock.Verify(x => x.Add(It.IsAny<Domain.Posts.PostComment>()), Times.Once);
        _postRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}