using Application.Common.Pagination;
using Application.Contracts.Persistence.Repositories.Post;
using Application.Contracts.Services;
using Application.Requests.Posts.Root.Queries.GetAllPaged;
using Application.Responses;
using ApplicationUnitTests.Factories;
using ApplicationUnitTests.Helpers;
using Moq;

namespace ApplicationUnitTests.Requests.Posts.Root.Queries.GetAllPaged;

public class GetAllPostsPagedHandlerTest
{
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetAllPostsPagedHandler _handler;
    
    private readonly List<PostResponseDto> _posts;

    public GetAllPostsPagedHandlerTest()
    {
        _postRepositoryMock = new Mock<IPostRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _handler = new GetAllPostsPagedHandler(_postRepositoryMock.Object, _cacheServiceMock.Object);
        
        _posts = MapperHelper.GetMapper().Map<List<PostResponseDto>>(TestDataFactory.CreatePosts(5));
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResult_FromDatabase()
    {
        // Arrange
        var query = new GetAllPostsPagedQuery()
        {
            PageNumber = 1,
            PageSize = 10
        };

        var pagedResult = PagedResult<PostResponseDto>.Create(_posts, 5, 1, 10);

        _cacheServiceMock
            .Setup(x => x.GetAsync<PagedResult<PostResponseDto>>(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync((PagedResult<PostResponseDto>?)null);
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
        
        _cacheServiceMock.Verify(x => x.GetAsync<PagedResult<PostResponseDto>>(It.IsAny<string>(), CancellationToken.None), Times.Once);
        _postRepositoryMock.Verify(x => x.GetDtoPagedAsync(query.PageNumber, query.PageSize), Times.Once);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnPagedResult_FromCache()
    {
        // Arrange
        var query = new GetAllPostsPagedQuery()
        {
            PageNumber = 1,
            PageSize = 10
        };

        var pagedResult = PagedResult<PostResponseDto>.Create(_posts, 5, 1, 10);

        _cacheServiceMock
            .Setup(x => x.GetAsync<PagedResult<PostResponseDto>>(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(pagedResult);
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
        
        _cacheServiceMock.Verify(x => x.GetAsync<PagedResult<PostResponseDto>>(It.IsAny<string>(), CancellationToken.None), Times.Once);
        _postRepositoryMock.Verify(x => x.GetDtoPagedAsync(query.PageNumber, query.PageSize), Times.Never);
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

        _cacheServiceMock
            .Setup(x => x.GetAsync<PagedResult<PostResponseDto>>(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync((PagedResult<PostResponseDto>?)null);
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

        _cacheServiceMock.Verify(x => x.GetAsync<PagedResult<PostResponseDto>>(It.IsAny<string>(), CancellationToken.None), Times.Once);
        _postRepositoryMock.Verify(x => x.GetDtoPagedAsync(query.PageNumber, query.PageSize), Times.Once);
    }
}