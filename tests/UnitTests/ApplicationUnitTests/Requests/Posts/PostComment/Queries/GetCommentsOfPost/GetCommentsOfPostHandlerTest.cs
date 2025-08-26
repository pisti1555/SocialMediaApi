using Application.Common.Interfaces.Repositories.Post;
using Application.Requests.Posts.PostComment.Queries.GetCommentsOfPost;
using ApplicationUnitTests.Common;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts.Factories;
using Moq;

namespace ApplicationUnitTests.Requests.Posts.PostComment.Queries.GetCommentsOfPost;

public class GetCommentsOfPostHandlerTest
{
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly Mock<IPostCommentRepository> _commentRepositoryMock;
    private readonly GetCommentsOfPostHandler _handler;

    public GetCommentsOfPostHandlerTest()
    {
        _postRepositoryMock = new Mock<IPostRepository>();
        _commentRepositoryMock = new Mock<IPostCommentRepository>();
        _postRepositoryMock.SetupGet(x => x.CommentRepository).Returns(_commentRepositoryMock.Object);
        
        _handler = new GetCommentsOfPostHandler(_postRepositoryMock.Object, TestMapperSetup.SetupMapper());
    }

    [Fact]
    public async Task Handle_ShouldReturnOkList()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);
        var comment = PostCommentFactory.Create("This is a test comment", user, post);
        var comments = new List<Domain.Posts.PostComment> { comment };

        var query = new GetCommentsOfPostQuery(post.Id.ToString());

        _postRepositoryMock.Setup(x => x.ExistsAsync(post.Id)).ReturnsAsync(true);
        _commentRepositoryMock.Setup(x => x.GetAllCommentOfPostAsync(post.Id)).ReturnsAsync(comments);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal(comment.Id, result[0].Id);
        Assert.Equal(comment.Text, result[0].Text);

        _postRepositoryMock.Verify(x => x.ExistsAsync(post.Id), Times.Once);
        _commentRepositoryMock.Verify(x => x.GetAllCommentOfPostAsync(post.Id), Times.Once);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnEmptyList()
    {
        // Arrange
        var user = TestObjects.CreateTestUser();
        var post = TestObjects.CreateTestPost(user);

        var query = new GetCommentsOfPostQuery(post.Id.ToString());

        _postRepositoryMock.Setup(x => x.ExistsAsync(post.Id)).ReturnsAsync(true);
        _commentRepositoryMock.Setup(x => x.GetAllCommentOfPostAsync(post.Id)).ReturnsAsync(new List<Domain.Posts.PostComment>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        _postRepositoryMock.Verify(x => x.ExistsAsync(post.Id), Times.Once);
        _commentRepositoryMock.Verify(x => x.GetAllCommentOfPostAsync(post.Id), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_PostNotFound()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var query = new GetCommentsOfPostQuery(postId.ToString());

        _postRepositoryMock.Setup(x => x.ExistsAsync(postId)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));

        _postRepositoryMock.Verify(x => x.ExistsAsync(postId), Times.Once);
        _commentRepositoryMock.Verify(x => x.GetAllCommentOfPostAsync(It.IsAny<Guid>()), Times.Never);
    }
}