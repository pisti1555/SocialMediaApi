using Application.Requests.Posts.PostComment.Queries.GetCommentsOfPost;
using Application.Responses;
using ApplicationUnitTests.Factories;
using ApplicationUnitTests.Requests.Posts.Common;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Moq;

namespace ApplicationUnitTests.Requests.Posts.PostComment.Queries;

public class GetCommentsOfPostHandlerTest : BasePostHandlerTest
{
    private readonly GetCommentsOfPostHandler _handler;
    
    private readonly Post _post;
    private readonly string _postCommentsCacheKey;
    private readonly List<Domain.Posts.PostComment> _comments;
    private readonly List<PostCommentResponseDto> _cachedCommentsOfPost;

    public GetCommentsOfPostHandlerTest()
    {
        _handler = new GetCommentsOfPostHandler(PostRepositoryMock.Object, CacheServiceMock.Object, Mapper);
        
        _post = TestDataFactory.CreatePost(null, true);
        _comments = TestDataFactory.CreateComments(5, _post);
        
        _postCommentsCacheKey = $"post-comments-{_post.Id.ToString()}";
        _cachedCommentsOfPost = Mapper.Map<List<PostCommentResponseDto>>(_comments);
    }

    private GetCommentsOfPostQuery CreateQuery() => new(_post.Id.ToString());

    [Fact]
    public async Task Handle_WhenNoCache_ShouldReturnOkListFromDatabase()
    {
        // Arrange
        SetupPostExists(_post.Id, true);
        SetupCache<List<PostCommentResponseDto>?>(_postCommentsCacheKey, null);
        SetupComments(_comments, _post.Id);

        // Act
        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        // Assert
        PostRepositoryMock.Verify(x => x.ExistsAsync(_post.Id), Times.Once);
        CacheServiceMock.Verify(x => x.GetAsync<List<PostCommentResponseDto>>(_postCommentsCacheKey, CancellationToken.None), Times.Once);
        CommentRepositoryMock.Verify(x => x.GetAllCommentOfPostAsync(_post.Id), Times.Once);
        
        AssertCommentsMatch(_comments, result);
    }
    
    [Fact]
    public async Task Handle_WhenCacheExists_ShouldReturnOkListFromCache()
    {
        // Arrange
        SetupPostExists(_post.Id, true);
        SetupCache<List<PostCommentResponseDto>?>(_postCommentsCacheKey, _cachedCommentsOfPost);

        // Act
        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        // Assert
        PostRepositoryMock.Verify(x => x.ExistsAsync(_post.Id), Times.Once);
        CacheServiceMock.Verify(x => x.GetAsync<List<PostCommentResponseDto>>(_postCommentsCacheKey, CancellationToken.None), Times.Once);
        CommentRepositoryMock.Verify(x => x.GetAllCommentOfPostAsync(_post.Id), Times.Never);
        
        AssertCommentsMatch(_comments, result);
    }
    
    [Fact]
    public async Task Handle_WhenNoComments_ShouldReturnEmptyList()
    {
        // Arrange
        SetupPostExists(_post.Id, true);
        SetupCache<List<PostCommentResponseDto>?>(_postCommentsCacheKey, null);
        SetupComments([], _post.Id);
        
        // Act
        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        // Assert
        PostRepositoryMock.Verify(x => x.ExistsAsync(_post.Id), Times.Once);
        CacheServiceMock.Verify(x => x.GetAsync<List<PostCommentResponseDto>>(_postCommentsCacheKey, CancellationToken.None), Times.Once);
        CommentRepositoryMock.Verify(x => x.GetAllCommentOfPostAsync(_post.Id), Times.Once);
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        SetupPostExists(_post.Id, false);
        
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(CreateQuery(), CancellationToken.None));

        PostRepositoryMock.Verify(x => x.ExistsAsync(_post.Id), Times.Once);
        CacheServiceMock.Verify(x => x.GetAsync<List<PostCommentResponseDto>>(It.IsAny<string>(), CancellationToken.None), Times.Never);
        CommentRepositoryMock.Verify(x => x.GetAllCommentOfPostAsync(It.IsAny<Guid>()), Times.Never);
    }
}