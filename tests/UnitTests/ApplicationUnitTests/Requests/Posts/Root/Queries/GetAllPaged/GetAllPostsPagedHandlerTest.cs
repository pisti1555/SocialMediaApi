using Application.Common.Pagination;
using Application.Contracts.Persistence.Repositories.Post;
using Application.Requests.Posts.Root.Queries.GetAllPaged;
using Application.Responses;
using ApplicationUnitTests.Common;
using Moq;

namespace ApplicationUnitTests.Requests.Posts.Root.Queries.GetAllPaged;

public class GetAllPostsPagedHandlerTest
{
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly GetAllPostsPagedHandler _handler;

    public GetAllPostsPagedHandlerTest()
    {
        _postRepositoryMock = new Mock<IPostRepository>();
        _handler = new GetAllPostsPagedHandler(_postRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSucceed()
    {
        // Arrange
        var query = new GetAllPostsPagedQuery()
        {
            PageNumber = 1,
            PageSize = 10
        };
        
        var user = TestObjects.CreateTestUser();

        var items = new List<PostResponseDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                UserName = user.UserName,
                Text = "Post text 1",
                Comments = [],
                CommentsCount = 0,
                Likes = [],
                LikesCount = 0,
                CreatedAt = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                UserName = user.UserName,
                Text = "Post text 2",
                Comments = [],
                CommentsCount = 0,
                Likes = [],
                LikesCount = 0,
                CreatedAt = DateTime.UtcNow,
            }
        };

        var pagedResult = PagedResult<PostResponseDto>.Create(items, 2, 1, 10);

        _postRepositoryMock
            .Setup(x => x.GetDtoPagedAsync(query.PageNumber, query.PageSize))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Equal(pagedResult.TotalCount, result.TotalCount);
        Assert.Equal(pagedResult.Items.Count, result.Items.Count);
        Assert.Equal(pagedResult.Items[0].Id, result.Items[0].Id);
        Assert.Equal(pagedResult.Items[0].Text, result.Items[0].Text);
        Assert.Equal(pagedResult.Items[0].UserName, result.Items[0].UserName);

        _postRepositoryMock.Verify(x => x.GetDtoPagedAsync(query.PageNumber, query.PageSize), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_EmptyResult()
    {
        // Arrange
        var query = new GetAllPostsPagedQuery
        {
            PageNumber = 1,
            PageSize = 10
        };
        
        var pagedResult = PagedResult<PostResponseDto>.Create(new List<PostResponseDto>(), 0, 1, 10);

        _postRepositoryMock
            .Setup(x => x.GetDtoPagedAsync(query.PageNumber, query.PageSize))
            .ReturnsAsync(pagedResult);
        
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Empty(result.Items);
        Assert.Equal(pagedResult.TotalCount, result.TotalCount);
        Assert.Equal(pagedResult.Items.Count, result.Items.Count);
        Assert.Empty(pagedResult.Items);

        _postRepositoryMock.Verify(x => x.GetDtoPagedAsync(query.PageNumber, query.PageSize), Times.Once);
    }
}