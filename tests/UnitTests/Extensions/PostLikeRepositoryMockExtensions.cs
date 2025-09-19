using Application.Contracts.Persistence.Repositories.Post;
using Moq;

namespace UnitTests.Extensions;

public static class PostLikeRepositoryMockExtensions
{
    public static void SetupLikes(this Mock<IPostLikeRepository> likeRepositoryMock, IEnumerable<global::Domain.Posts.PostLike> likes, Guid postId) =>
        likeRepositoryMock.Setup(x => x.GetAllOfPostAsync(postId)).ReturnsAsync(likes.ToList());
    
    public static void SetupLikeByUserIdAndPostId(this Mock<IPostLikeRepository> likeRepositoryMock, Guid userId, Guid postId, global::Domain.Posts.PostLike? like) =>
        likeRepositoryMock.Setup(x => x.GetByUserIdAndPostIdAsync(userId, postId))
            .ReturnsAsync(like);
    
    public static void SetupLikeExists(this Mock<IPostLikeRepository> likeRepositoryMock, Guid postId, Guid userId, bool exists) =>
        likeRepositoryMock.Setup(x => x.ExistsAsync(postId, userId))
            .ReturnsAsync(exists);
}