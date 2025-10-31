using Application.Requests.Posts.Root.Queries.GetById;
using Application.Responses;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Moq;
using UnitTests.Application.Posts.Common;
using UnitTests.Extensions;
using UnitTests.Factories;

namespace UnitTests.Application.Posts.Root.Queries;

public class GetPostByIdHandlerTest : BasePostHandlerTest
{
    private readonly GetPostByIdHandler _handler;

    private readonly Post _post;
    private readonly PostResponseDto _cachedPost;
    private readonly string _postCacheKey;

    public GetPostByIdHandlerTest()
    {
        _handler = new GetPostByIdHandler(PostQueryRepositoryMock.Object, CacheServiceMock.Object);

        _post = TestDataFactory.CreatePostWithRelations().Post;
        _cachedPost = Mapper.Map<PostResponseDto>(_post);
        _postCacheKey = $"post-{_post.Id.ToString()}";
    }
    
    private GetPostByIdQuery CreateQuery() => new(_post.Id.ToString());
    
    private static void AssertPostMatch(Post expected, PostResponseDto actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Text, actual.Text);
        Assert.Equal(expected.CreatedAt, actual.CreatedAt);
        Assert.Equal(expected.User.Id, actual.UserId);
        Assert.Equal(expected.User.UserName, actual.UserName);
        Assert.Equal(expected.Comments.Count, actual.CommentsCount);
        Assert.Equal(expected.Likes.Count, actual.LikesCount);
    }

    [Fact]
    public async Task Handle_WhenNoCache_ShouldReturnPostFromDatabase()
    {
        // Arrange
        CacheServiceMock.SetupCache<PostResponseDto?>(_postCacheKey, null);
        PostQueryRepositoryMock.SetupPost(_post, Mapper);

        // Act
        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        // Assert
        CacheServiceMock.VerifyCacheHit<PostResponseDto?>(_postCacheKey);
        PostQueryRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id, It.IsAny<CancellationToken>()), Times.Once);
        
        AssertPostMatch(_post, result);
    }

    [Fact]
    public async Task Handle_WhenCacheExists_ShouldReturnPostFromCache()
    {
        // Arrange
        CacheServiceMock.SetupCache<PostResponseDto?>(_postCacheKey, _cachedPost);
        PostQueryRepositoryMock.SetupPost(_post, Mapper);

        // Act
        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);
        
        CacheServiceMock.VerifyCacheHit<PostResponseDto?>(_postCacheKey);
        PostQueryRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id, It.IsAny<CancellationToken>()), Times.Never);

        // Assert
        AssertPostMatch(_post, result);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        CacheServiceMock.SetupCache<PostResponseDto?>(_postCacheKey, null);
        PostQueryRepositoryMock.SetupPost(null, Mapper);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(CreateQuery(), CancellationToken.None));
        
        CacheServiceMock.VerifyCacheHit<PostResponseDto?>(_postCacheKey);
        PostQueryRepositoryMock.Verify(x => x.GetByIdAsync(_post.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenGuidIsInvalid_ShouldThrowBadRequestException()
    {
        // Arrange
        var invalidQuery = new GetPostByIdQuery("invalid");
        
        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(invalidQuery, CancellationToken.None));
        
        CacheServiceMock.VerifyCacheHit<PostResponseDto?>(_postCacheKey, false);
        PostQueryRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}