using Application.Requests.Posts.PostLike.Queries.GetLikesOfPost;
using Application.Responses;
using ApplicationUnitTests.Factories;
using ApplicationUnitTests.Requests.Posts.Common;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Moq;

namespace ApplicationUnitTests.Requests.Posts.PostLike.Queries;

public class GetLikesOfPostHandlerTest : BasePostHandlerTest
{
    
    private readonly GetLikesOfPostHandler _handler;
    private readonly Post _post;
    private readonly List<Domain.Posts.PostLike> _likes;
    private readonly string _postLikesCacheKey;
    private readonly List<PostLikeResponseDto> _cachedLikesOfPost;

    public GetLikesOfPostHandlerTest()
    {
        _handler = new GetLikesOfPostHandler(PostRepositoryMock.Object, CacheServiceMock.Object, Mapper);

        _post = TestDataFactory.CreatePost(null, true);
        _likes = TestDataFactory.CreateLikes(5, _post, null, true);
        
        _postLikesCacheKey = $"post-likes-{_post.Id.ToString()}";
        _cachedLikesOfPost = Mapper.Map<List<PostLikeResponseDto>>(_likes);
    }
    
    private GetLikesOfPostQuery CreateQuery() => new(_post.Id.ToString());

    [Fact]
    public async Task Handle_WhenNoCache_ShouldReturnOkListFromDatabase()
    {
        // Arrange
        SetupPostExists(_post.Id, true);
        SetupCache<List<PostLikeResponseDto>?>(_postLikesCacheKey, null);
        SetupLikes(_likes, _post.Id);
        
        // Act
        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);
        
        // Assert
        PostRepositoryMock.Verify(x => x.ExistsAsync(_post.Id), Times.Once);
        CacheServiceMock.Verify(x => x.GetAsync<List<PostLikeResponseDto>>(_postLikesCacheKey, CancellationToken.None), Times.Once);
        LikeRepositoryMock.Verify(x => x.GetAllOfPostAsync(_post.Id), Times.Once);
        
        AssertLikesMatch(_likes, result);
    }
    
    [Fact]
    public async Task Handle_WhenCacheExists_ShouldReturnOkListFromCache()
    {
        // Arrange
        SetupPostExists(_post.Id, true);
        SetupCache<List<PostLikeResponseDto>?>(_postLikesCacheKey, _cachedLikesOfPost);
        
        // Act
        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);
        
        // Assert
        PostRepositoryMock.Verify(x => x.ExistsAsync(_post.Id), Times.Once);
        CacheServiceMock.Verify(x => x.GetAsync<List<PostLikeResponseDto>>(_postLikesCacheKey, CancellationToken.None), Times.Once);
        LikeRepositoryMock.Verify(x => x.GetAllOfPostAsync(_post.Id), Times.Never);
        
        AssertLikesMatch(_likes, result);
    }
    
    [Fact]
    public async Task Handle_WhenNoLikes_ShouldReturnEmptyList()
    {
        // Arrange
        SetupPostExists(_post.Id, true);
        SetupCache<List<PostLikeResponseDto>?>(_postLikesCacheKey, null);
        SetupLikes([], _post.Id);
        
        // Act
        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);
        
        // Assert
        PostRepositoryMock.Verify(x => x.ExistsAsync(_post.Id), Times.Once);
        CacheServiceMock.Verify(x => x.GetAsync<List<PostLikeResponseDto>>(_postLikesCacheKey, CancellationToken.None), Times.Once);
        LikeRepositoryMock.Verify(x => x.GetAllOfPostAsync(_post.Id), Times.Once);
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task Handle_WhenNoPost_ShouldThrowNotFoundException()
    {
        // Arrange
        SetupPostExists(_post.Id, false);
        
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(CreateQuery(), CancellationToken.None));

        PostRepositoryMock.Verify(x => x.ExistsAsync(_post.Id), Times.Once);
        CacheServiceMock.Verify(x => x.GetAsync<List<PostLikeResponseDto>>(It.IsAny<string>(), CancellationToken.None), Times.Never);
        LikeRepositoryMock.Verify(x => x.GetAllOfPostAsync(It.IsAny<Guid>()), Times.Never);
    }
}