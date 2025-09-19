using Application.Contracts.Persistence.Repositories.AppUser;
using Application.Contracts.Persistence.Repositories.Post;
using Moq;
using UnitTests.Common;

namespace UnitTests.Application.Posts.Common;

public abstract class BasePostHandlerTest : TestBase
{
    protected readonly Mock<IAppUserRepository> UserRepositoryMock = new();
    protected readonly Mock<IPostRepository> PostRepositoryMock = new();
    protected readonly Mock<IPostCommentRepository> CommentRepositoryMock = new();
    protected readonly Mock<IPostLikeRepository> LikeRepositoryMock = new();
    
    protected BasePostHandlerTest()
    {
        PostRepositoryMock.SetupGet(x => x.CommentRepository).Returns(CommentRepositoryMock.Object);
        PostRepositoryMock.SetupGet(x => x.LikeRepository).Returns(LikeRepositoryMock.Object);
    }
}