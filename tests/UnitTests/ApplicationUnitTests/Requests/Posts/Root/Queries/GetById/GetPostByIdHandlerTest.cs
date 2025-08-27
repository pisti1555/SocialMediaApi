using Application.Contracts.Persistence.Repositories.Post;
using Application.Requests.Posts.Root.Queries.GetById;
using ApplicationUnitTests.Common;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Moq;

namespace ApplicationUnitTests.Requests.Posts.Root.Queries.GetById;

public class GetPostByIdHandlerTest
{
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly GetPostByIdHandler _handler;

    public GetPostByIdHandlerTest()
    {
        _postRepositoryMock = new Mock<IPostRepository>();
        _handler = new GetPostByIdHandler(_postRepositoryMock.Object, TestMapperSetup.SetupMapper());
    }

    [Fact]
    public async Task Handle_ShouldSucceed()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var query = new GetPostByIdQuery(postId.ToString());

        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);

        _postRepositoryMock
            .Setup(x => x.GetByIdAsync(postId))
            .ReturnsAsync(post);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(result.Id, post.Id);
        Assert.Equal(result.Text, post.Text);
        Assert.Equal(result.CreatedAt, post.CreatedAt);
        Assert.Equal(result.UserId, user.Id);
        Assert.Equal(result.UserName, user.UserName);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_PostDoesNotExist()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var query = new GetPostByIdQuery(postId.ToString());

        _postRepositoryMock
            .Setup(x => x.GetByIdAsync(postId))
            .ReturnsAsync((Post?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_GUIDIsInvalid()
    {
        // Arrange
        var query = new GetPostByIdQuery("invalid-guid");

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(query, CancellationToken.None));
    }
}