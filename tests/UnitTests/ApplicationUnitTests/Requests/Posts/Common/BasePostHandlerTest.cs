using Application.Contracts.Persistence.Repositories.AppUser;
using Application.Contracts.Persistence.Repositories.Post;
using Application.Contracts.Services;
using Application.Responses;
using ApplicationUnitTests.Helpers;
using AutoMapper;
using Domain.Posts;
using Domain.Users;
using Moq;

namespace ApplicationUnitTests.Requests.Posts.Common;

public abstract class BasePostHandlerTest
{
    protected readonly Mock<IAppUserRepository> UserRepositoryMock = new();
    protected readonly Mock<IPostRepository> PostRepositoryMock = new();
    protected readonly Mock<IPostCommentRepository> CommentRepositoryMock = new();
    protected readonly Mock<IPostLikeRepository> LikeRepositoryMock = new();
    protected readonly Mock<ICacheService> CacheServiceMock = new();
    protected readonly IMapper Mapper;
    
    protected BasePostHandlerTest()
    {
        Mapper = MapperHelper.GetMapper();
        
        PostRepositoryMock.SetupGet(x => x.CommentRepository).Returns(CommentRepositoryMock.Object);
        PostRepositoryMock.SetupGet(x => x.LikeRepository).Returns(LikeRepositoryMock.Object);
    }
    
    // Setups
    protected void SetupCache<T>(string cacheKey, T? cachedData) =>
        CacheServiceMock.Setup(x => x.GetAsync<T>(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedData);
    
    protected void SetupPost(Post? post) =>
        PostRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<Guid>(id => post != null && id == post.Id)))
            .ReturnsAsync(post);
    
    protected void SetupLikes(IEnumerable<Domain.Posts.PostLike> likes, Guid postId) =>
        LikeRepositoryMock.Setup(x => x.GetAllOfPostAsync(postId)).ReturnsAsync(likes.ToList());
    
    protected void SetupLikeByUserIdAndPostId(Guid userId, Guid postId, Domain.Posts.PostLike? like) =>
        LikeRepositoryMock.Setup(x => x.GetByUserIdAndPostIdAsync(userId, postId))
            .ReturnsAsync(like);
    
    protected void SetupLikeExists(Guid postId, Guid userId, bool exists) =>
        LikeRepositoryMock.Setup(x => x.ExistsAsync(postId, userId))
            .ReturnsAsync(exists);
    
    protected void SetupComment(Domain.Posts.PostComment? comment) =>
        CommentRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<Guid>(id => comment != null && id == comment.Id)))
            .ReturnsAsync(comment);
    
    protected void SetupComments(IEnumerable<Domain.Posts.PostComment> comments, Guid postId) =>
        CommentRepositoryMock.Setup(x => x.GetAllCommentOfPostAsync(postId)).ReturnsAsync(comments.ToList());

    protected void SetupUser(AppUser? user) =>
        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<Guid>(id => user != null && id == user.Id)))
            .ReturnsAsync(user);

    protected void SetupSaveChanges(bool success) =>
        PostRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(success);
    
    protected void SetupPostExists(Guid postId, bool exists) =>
        PostRepositoryMock.Setup(x => x.ExistsAsync(postId)).ReturnsAsync(exists);
    
    // Verifications
    protected void VerifyLikeAdded(Post post, DateTime lastInteraction)
    {
        LikeRepositoryMock.Verify(x => x.Add(It.IsAny<Domain.Posts.PostLike>()), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        CacheServiceMock.Verify(x => x.RemoveAsync($"post-likes-{post.Id}", CancellationToken.None), Times.Once);
        Assert.NotEqual(lastInteraction, post.LastInteraction);
    }

    protected void VerifyLikeNotAdded(Post post, DateTime lastInteraction)
    {
        LikeRepositoryMock.Verify(x => x.Add(It.IsAny<Domain.Posts.PostLike>()), Times.Never);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        CacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), CancellationToken.None), Times.Never);
        Assert.Equal(lastInteraction, post.LastInteraction);
    }
    
    protected void VerifyLikeDeleted(Domain.Posts.PostLike like, Post post, DateTime lastInteraction)
    {
        LikeRepositoryMock.Verify(x => x.Delete(like), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        CacheServiceMock.Verify(x => x.RemoveAsync($"post-likes-{post.Id}", CancellationToken.None), Times.Once);
        Assert.NotEqual(lastInteraction, post.LastInteraction);
    }

    protected void VerifyLikeNotDeleted(Post post, DateTime lastInteraction)
    {
        LikeRepositoryMock.Verify(x => x.Delete(It.IsAny<Domain.Posts.PostLike>()), Times.Never);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        CacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), CancellationToken.None), Times.Never);
        Assert.Equal(lastInteraction, post.LastInteraction);
    }
    
    protected void VerifyCommentAdded(Post post, DateTime lastInteraction)
    {
        CommentRepositoryMock.Verify(x => x.Add(It.IsAny<Domain.Posts.PostComment>()), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        CacheServiceMock.Verify(x => x.RemoveAsync($"post-comments-{post.Id}", CancellationToken.None), Times.Once);
        Assert.NotEqual(lastInteraction, post.LastInteraction);
    }

    protected void VerifyCommentNotAdded(Post post, DateTime lastInteraction)
    {
        CommentRepositoryMock.Verify(x => x.Add(It.IsAny<Domain.Posts.PostComment>()), Times.Never);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        CacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), CancellationToken.None), Times.Never);
        Assert.Equal(lastInteraction, post.LastInteraction);
    }

    protected void VerifyCommentDeleted(Domain.Posts.PostComment comment, Post post, DateTime lastInteraction)
    {
        CommentRepositoryMock.Verify(x => x.Delete(comment), Times.Once);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        CacheServiceMock.Verify(x => x.RemoveAsync($"post-comments-{post.Id}", CancellationToken.None), Times.Once);
        Assert.NotEqual(lastInteraction, post.LastInteraction);
    }
    
    protected void VerifyCommentNotDeleted(Post post, DateTime lastInteraction)
    {
        CommentRepositoryMock.Verify(x => x.Delete(It.IsAny<Domain.Posts.PostComment>()), Times.Never);
        PostRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        CacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), CancellationToken.None), Times.Never);
        Assert.Equal(lastInteraction, post.LastInteraction);
    }
    
    // Assertions
    protected static void AssertCommentsMatch(List<Domain.Posts.PostComment> expected, List<PostCommentResponseDto> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Id, actual[i].Id);
            Assert.Equal(expected[i].Text, actual[i].Text);
            Assert.Equal(expected[i].UserId, actual[i].UserId);
            Assert.Equal(expected[i].PostId, actual[i].PostId);
        }
    }

    protected static void AssertLikesMatch(List<Domain.Posts.PostLike> expected, List<PostLikeResponseDto> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Id, actual[i].Id);
            Assert.Equal(expected[i].UserId, actual[i].UserId);
            Assert.Equal(expected[i].PostId, actual[i].PostId);
        }
    }
}