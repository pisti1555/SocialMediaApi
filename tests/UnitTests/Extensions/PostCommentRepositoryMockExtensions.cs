using Application.Contracts.Persistence.Repositories.Post;
using Moq;
using PostCommentEntity = Domain.Posts.PostComment;

namespace UnitTests.Extensions;

public static class PostCommentRepositoryMockExtensions
{
    public static void SetupComment(this Mock<IPostCommentRepository> commentRepositoryMock, PostCommentEntity? comment) =>
        commentRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<Guid>(id => comment != null && id == comment.Id)))
            .ReturnsAsync(comment);
    
    public static void SetupComments(this Mock<IPostCommentRepository> commentRepositoryMock, IEnumerable<PostCommentEntity> comments, Guid postId) =>
        commentRepositoryMock.Setup(x => x.GetAllCommentOfPostAsync(postId)).ReturnsAsync(comments.ToList());
}