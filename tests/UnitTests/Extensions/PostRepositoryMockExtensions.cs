using Application.Contracts.Persistence.Repositories.Post;
using Domain.Posts;
using Moq;

namespace UnitTests.Extensions;

public static class PostRepositoryMockExtensions
{
    public static void SetupPost(this Mock<IPostRepository> postRepositoryMock, Post? post)
    {
        postRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<Guid>(id => post != null && id == post.Id)))
            .ReturnsAsync(post);
    }
    
    public static void SetupPostExists(this Mock<IPostRepository> postRepositoryMock, Guid postId, bool exists)
    {
        postRepositoryMock
            .Setup(x => x.ExistsAsync(postId))
            .ReturnsAsync(exists);
    }

    public static void SetupSaveChanges(this Mock<IPostRepository> postRepositoryMock, bool success = true)
    {
        postRepositoryMock.Setup(x => x.SaveChangesAsync())
                    .ReturnsAsync(success);
    }

    public static void VerifyGetByIdQueryRan(this Mock<IPostRepository> postRepositoryMock)
    {
        postRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    }

    public static void VerifySaveChanges(this Mock<IPostRepository> postRepositoryMock, bool success)
    {
        postRepositoryMock.Verify(x => x.SaveChangesAsync(), success ? Times.Once : Times.Never);
    }
}