using Application.Requests.Posts.PostLike.Queries.GetLikesOfPost;
using Application.Responses;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Moq;
using UnitTests.Application.Posts.Common;
using UnitTests.Extensions;
using UnitTests.Factories;
using XPostLike = Domain.Posts.PostLike;

namespace UnitTests.Application.Posts.PostLike.Queries;

public class GetLikesOfPostHandlerTest : BasePostHandlerTest
{
    private readonly GetLikesOfPostHandler _handler;
    
    private readonly Post _post;
    private readonly List<XPostLike> _likes;
    private readonly string _postLikesCacheKey;
    private readonly List<PostLikeResponseDto> _cachedLikesOfPost;
    private readonly GetLikesOfPostQuery _query;

    public GetLikesOfPostHandlerTest()
    {
        _handler = new GetLikesOfPostHandler(PostRepositoryMock.Object, CacheServiceMock.Object, Mapper);
        
        _post = TestDataFactory.CreatePostWithRelations().Post;
        _likes = _post.Likes.ToList();
        
        _postLikesCacheKey = $"post-likes-{_post.Id.ToString()}";
        _cachedLikesOfPost = Mapper.Map<List<PostLikeResponseDto>>(_likes);
        _query = new GetLikesOfPostQuery(_post.Id.ToString());
    }
    
    private static void AssertLikesMatch(List<XPostLike> expected, List<PostLikeResponseDto> actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Count, actual.Count);
        
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Id, actual[i].Id);
            Assert.Equal(expected[i].UserId, actual[i].UserId);
            Assert.Equal(expected[i].User.Id, actual[i].UserId);
            Assert.Equal(expected[i].PostId, actual[i].PostId);
            Assert.Equal(expected[i].Post.Id, actual[i].PostId);
        }
    }

    [Fact]
    public async Task Handle_WhenNoCache_ShouldReturnOkListFromDatabase()
    {
        // Arrange
        CacheServiceMock.SetupCache<List<PostLikeResponseDto>?>(_postLikesCacheKey, null);
        PostRepositoryMock.SetupPostExists(_post.Id, true);
        LikeRepositoryMock.SetupLikes(_likes, _post.Id);
        
        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);
        
        // Assert
        CacheServiceMock.VerifyCacheHit<List<PostLikeResponseDto>?>(_postLikesCacheKey);
        PostRepositoryMock.Verify(x => x.ExistsAsync(_post.Id), Times.Once);
        LikeRepositoryMock.Verify(x => x.GetAllOfPostAsync(_post.Id), Times.Once);
        
        AssertLikesMatch(_likes, result);
    }
    
    [Fact]
    public async Task Handle_WhenCacheExists_ShouldReturnOkListFromCache()
    {
        // Arrange
        CacheServiceMock.SetupCache<List<PostLikeResponseDto>?>(_postLikesCacheKey, _cachedLikesOfPost);
        PostRepositoryMock.SetupPostExists(_post.Id, true);
        
        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);
        
        // Assert
        CacheServiceMock.VerifyCacheHit<List<PostLikeResponseDto>?>(_postLikesCacheKey);
        PostRepositoryMock.Verify(x => x.ExistsAsync(_post.Id), Times.Once);
        LikeRepositoryMock.Verify(x => x.GetAllOfPostAsync(_post.Id), Times.Never);
        
        AssertLikesMatch(_likes, result);
    }
    
    [Fact]
    public async Task Handle_WhenNoLikes_ShouldReturnEmptyList()
    {
        // Arrange
        CacheServiceMock.SetupCache<List<PostLikeResponseDto>?>(_postLikesCacheKey, null);
        PostRepositoryMock.SetupPostExists(_post.Id, true);
        LikeRepositoryMock.SetupLikes([], _post.Id);
        
        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);
        
        // Assert
        CacheServiceMock.VerifyCacheHit<List<PostLikeResponseDto>?>(_postLikesCacheKey);
        PostRepositoryMock.Verify(x => x.ExistsAsync(_post.Id), Times.Once);
        LikeRepositoryMock.Verify(x => x.GetAllOfPostAsync(_post.Id), Times.Once);
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task Handle_WhenNoPost_ShouldThrowNotFoundException()
    {
        // Arrange
        PostRepositoryMock.SetupPostExists(_post.Id, false);
        
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(_query, CancellationToken.None));

        CacheServiceMock.VerifyCacheHit<List<PostLikeResponseDto>?>(_postLikesCacheKey, false);
        PostRepositoryMock.Verify(x => x.ExistsAsync(_post.Id), Times.Once);
        LikeRepositoryMock.Verify(x => x.GetAllOfPostAsync(It.IsAny<Guid>()), Times.Never);
    }
}