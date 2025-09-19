using Application.Requests.Posts.PostComment.Queries.GetCommentsOfPost;
using Application.Responses;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Moq;
using UnitTests.Application.Posts.Common;
using UnitTests.Extensions;
using UnitTests.Factories;
using XPostComment = Domain.Posts.PostComment;

namespace UnitTests.Application.Posts.PostComment.Queries;

public class GetCommentsOfPostHandlerTest : BasePostHandlerTest
{
    private readonly GetCommentsOfPostHandler _handler;
    
    private readonly Post _post;
    private readonly List<XPostComment> _comments;
    private readonly string _postCommentsCacheKey;
    private readonly List<PostCommentResponseDto> _cachedCommentsOfPost;
    private readonly GetCommentsOfPostQuery _query;

    public GetCommentsOfPostHandlerTest()
    {
        _handler = new GetCommentsOfPostHandler(PostRepositoryMock.Object, CacheServiceMock.Object, Mapper);
        
        _post = TestDataFactory.CreatePostWithRelations(commentCount: 5).Post;
        _comments = _post.Comments.ToList();
        
        _postCommentsCacheKey = $"post-comments-{_post.Id.ToString()}";
        _cachedCommentsOfPost = Mapper.Map<List<PostCommentResponseDto>>(_comments);
        _query = new GetCommentsOfPostQuery(_post.Id.ToString());
    }
    
    private static void AssertCommentsMatch(List<XPostComment> expected, List<PostCommentResponseDto> actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Count, actual.Count);
        
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Id, actual[i].Id);
            Assert.Equal(expected[i].Text, actual[i].Text);
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
        CacheServiceMock.SetupCache<List<PostCommentResponseDto>?>(_postCommentsCacheKey, null);
        PostRepositoryMock.SetupPostExists(_post.Id, true);
        CommentRepositoryMock.SetupComments(_comments, _post.Id);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        CacheServiceMock.VerifyCacheHit<List<PostCommentResponseDto>?>(_postCommentsCacheKey);
        PostRepositoryMock.Verify(x => x.ExistsAsync(_post.Id), Times.Once);
        CommentRepositoryMock.Verify(x => x.GetAllCommentOfPostAsync(_post.Id), Times.Once);
        
        AssertCommentsMatch(_comments, result);
    }
    
    [Fact]
    public async Task Handle_WhenCacheExists_ShouldReturnOkListFromCache()
    {
        // Arrange
        CacheServiceMock.SetupCache<List<PostCommentResponseDto>?>(_postCommentsCacheKey, _cachedCommentsOfPost);
        PostRepositoryMock.SetupPostExists(_post.Id, true);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        CacheServiceMock.VerifyCacheHit<List<PostCommentResponseDto>?>(_postCommentsCacheKey);
        PostRepositoryMock.Verify(x => x.ExistsAsync(_post.Id), Times.Once);
        CommentRepositoryMock.Verify(x => x.GetAllCommentOfPostAsync(_post.Id), Times.Never);
        
        AssertCommentsMatch(_comments, result);
    }
    
    [Fact]
    public async Task Handle_WhenNoComments_ShouldReturnEmptyList()
    {
        // Arrange
        CacheServiceMock.SetupCache<List<PostCommentResponseDto>?>(_postCommentsCacheKey, null);
        PostRepositoryMock.SetupPostExists(_post.Id, true);
        CommentRepositoryMock.SetupComments([], _post.Id);
        
        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        CacheServiceMock.VerifyCacheHit<List<PostCommentResponseDto>?>(_postCommentsCacheKey);
        PostRepositoryMock.Verify(x => x.ExistsAsync(_post.Id), Times.Once);
        CommentRepositoryMock.Verify(x => x.GetAllCommentOfPostAsync(_post.Id), Times.Once);
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WhenPostDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        PostRepositoryMock.SetupPostExists(_post.Id, false);
        
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(_query, CancellationToken.None));

        CacheServiceMock.VerifyCacheHit<List<PostCommentResponseDto>?>(_postCommentsCacheKey, false);
        PostRepositoryMock.Verify(x => x.ExistsAsync(_post.Id), Times.Once);
        CommentRepositoryMock.Verify(x => x.GetAllCommentOfPostAsync(It.IsAny<Guid>()), Times.Never);
    }
}