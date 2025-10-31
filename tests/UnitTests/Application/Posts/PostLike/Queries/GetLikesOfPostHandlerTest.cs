using System.Linq.Expressions;
using Application.Requests.Posts.PostLike.Queries.GetAllOfPost;
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
    private readonly List<PostLikeResponseDto> _likes;
    private readonly string _postLikesCacheKey;
    private readonly GetLikesOfPostQuery _query;

    public GetLikesOfPostHandlerTest()
    {
        _handler = new GetLikesOfPostHandler(PostQueryRepositoryMock.Object, LikeQueryRepositoryMock.Object, CacheServiceMock.Object);
        
        _post = TestDataFactory.CreatePostWithRelations().Post;
        _likes = Mapper.Map<List<PostLikeResponseDto>>(_post.Likes);
        
        _postLikesCacheKey = $"post-likes-{_post.Id.ToString()}";
        _query = new GetLikesOfPostQuery(_post.Id.ToString());
    }
    
    private static void AssertLikesMatch(List<PostLikeResponseDto> expected, List<PostLikeResponseDto> actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Count, actual.Count);
        
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Id, actual[i].Id);
            Assert.Equal(expected[i].UserId, actual[i].UserId);
            Assert.Equal(expected[i].PostId, actual[i].PostId);
        }
    }

    [Fact]
    public async Task Handle_WhenNoCache_ShouldReturnOkListFromDatabase()
    {
        // Arrange
        CacheServiceMock.SetupCache<List<PostLikeResponseDto>?>(_postLikesCacheKey, null);
        PostQueryRepositoryMock.SetupPostExists(_post.Id, true);
        LikeQueryRepositoryMock.SetupLikes(_likes);
        
        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);
        
        // Assert
        CacheServiceMock.VerifyCacheHit<List<PostLikeResponseDto>?>(_postLikesCacheKey);
        PostQueryRepositoryMock.Verify(x => x.ExistsAsync(_post.Id, It.IsAny<CancellationToken>()), Times.Once);
        LikeQueryRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<Expression<Func<XPostLike, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        
        AssertLikesMatch(_likes, result);
    }
    
    [Fact]
    public async Task Handle_WhenCacheExists_ShouldReturnOkListFromCache()
    {
        // Arrange
        CacheServiceMock.SetupCache<List<PostLikeResponseDto>?>(_postLikesCacheKey, _likes);
        PostQueryRepositoryMock.SetupPostExists(_post.Id, true);
        
        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);
        
        // Assert
        CacheServiceMock.VerifyCacheHit<List<PostLikeResponseDto>?>(_postLikesCacheKey);
        PostQueryRepositoryMock.Verify(x => x.ExistsAsync(_post.Id, It.IsAny<CancellationToken>()), Times.Once);
        LikeQueryRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<Expression<Func<XPostLike, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
        
        AssertLikesMatch(_likes, result);
    }
    
    [Fact]
    public async Task Handle_WhenNoLikes_ShouldReturnEmptyList()
    {
        // Arrange
        CacheServiceMock.SetupCache<List<PostLikeResponseDto>?>(_postLikesCacheKey, null);
        PostQueryRepositoryMock.SetupPostExists(_post.Id, true);
        LikeQueryRepositoryMock.SetupLikes([]);
        
        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);
        
        // Assert
        CacheServiceMock.VerifyCacheHit<List<PostLikeResponseDto>?>(_postLikesCacheKey);
        PostQueryRepositoryMock.Verify(x => x.ExistsAsync(_post.Id, It.IsAny<CancellationToken>()), Times.Once);
        LikeQueryRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<Expression<Func<XPostLike, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        PostQueryRepositoryMock.SetupPostExists(_post.Id, false);
        
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(_query, CancellationToken.None));

        CacheServiceMock.VerifyCacheHit<List<PostLikeResponseDto>?>(_postLikesCacheKey, false);
        PostQueryRepositoryMock.Verify(x => x.ExistsAsync(_post.Id, It.IsAny<CancellationToken>()), Times.Once);
        LikeQueryRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<Expression<Func<XPostLike, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}