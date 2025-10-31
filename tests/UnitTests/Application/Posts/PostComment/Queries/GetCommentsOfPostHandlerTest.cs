using System.Linq.Expressions;
using Application.Requests.Posts.PostComment.Queries.GetAllOfPost;
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
    private readonly List<PostCommentResponseDto> _comments;
    private readonly string _postCommentsCacheKey;
    private readonly GetCommentsOfPostQuery _query;

    public GetCommentsOfPostHandlerTest()
    {
        _handler = new GetCommentsOfPostHandler(PostQueryRepositoryMock.Object, CommentQueryRepositoryMock.Object, CacheServiceMock.Object);
        
        _post = TestDataFactory.CreatePostWithRelations(commentCount: 5).Post;
        _comments = Mapper.Map<List<PostCommentResponseDto>>(_post.Comments);
        
        _postCommentsCacheKey = $"post-comments-{_post.Id.ToString()}";
        _query = new GetCommentsOfPostQuery(_post.Id.ToString());
    }
    
    private static void AssertCommentsMatch(List<PostCommentResponseDto> expected, List<PostCommentResponseDto> actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Count, actual.Count);
        
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Id, actual[i].Id);
            Assert.Equal(expected[i].Text, actual[i].Text);
            Assert.Equal(expected[i].UserId, actual[i].UserId);
            Assert.Equal(expected[i].PostId, actual[i].PostId);
        }
    }

    [Fact]
    public async Task Handle_WhenNoCache_ShouldReturnOkListFromDatabase()
    {
        // Arrange
        CacheServiceMock.SetupCache<List<PostCommentResponseDto>?>(_postCommentsCacheKey, null);
        PostQueryRepositoryMock.SetupPostExists(_post.Id, true);
        CommentQueryRepositoryMock.SetupComments(_comments);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        CacheServiceMock.VerifyCacheHit<List<PostCommentResponseDto>?>(_postCommentsCacheKey);
        PostQueryRepositoryMock.Verify(x => x.ExistsAsync(_post.Id, It.IsAny<CancellationToken>()), Times.Once);
        CommentQueryRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<Expression<Func<XPostComment, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        
        AssertCommentsMatch(_comments, result);
    }
    
    [Fact]
    public async Task Handle_WhenCacheExists_ShouldReturnOkListFromCache()
    {
        // Arrange
        CacheServiceMock.SetupCache<List<PostCommentResponseDto>?>(_postCommentsCacheKey, _comments);
        PostQueryRepositoryMock.SetupPostExists(_post.Id, true);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        CacheServiceMock.VerifyCacheHit<List<PostCommentResponseDto>?>(_postCommentsCacheKey);
        PostQueryRepositoryMock.Verify(x => x.ExistsAsync(_post.Id, It.IsAny<CancellationToken>()), Times.Once);
        CommentQueryRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<Expression<Func<XPostComment, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
        
        AssertCommentsMatch(_comments, result);
    }
    
    [Fact]
    public async Task Handle_WhenNoComments_ShouldReturnEmptyList()
    {
        // Arrange
        CacheServiceMock.SetupCache<List<PostCommentResponseDto>?>(_postCommentsCacheKey, null);
        PostQueryRepositoryMock.SetupPostExists(_post.Id, true);
        CommentQueryRepositoryMock.SetupComments([]);
        
        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        CacheServiceMock.VerifyCacheHit<List<PostCommentResponseDto>?>(_postCommentsCacheKey);
        PostQueryRepositoryMock.Verify(x => x.ExistsAsync(_post.Id, It.IsAny<CancellationToken>()), Times.Once);
        CommentQueryRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<Expression<Func<XPostComment, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        
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

        CacheServiceMock.VerifyCacheHit<List<PostCommentResponseDto>?>(_postCommentsCacheKey, false);
        PostQueryRepositoryMock.Verify(x => x.ExistsAsync(_post.Id, It.IsAny<CancellationToken>()), Times.Once);
        CommentQueryRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<Expression<Func<XPostComment, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}