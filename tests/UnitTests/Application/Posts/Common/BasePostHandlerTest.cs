using Application.Contracts.Persistence.Repositories;
using Application.Responses;
using Domain.Posts;
using Domain.Users;
using Moq;
using UnitTests.Common;
using XComment = Domain.Posts.PostComment;
using XLike = Domain.Posts.PostLike;

namespace UnitTests.Application.Posts.Common;

public abstract class BasePostHandlerTest : TestBase
{
    protected readonly Mock<IRepository<AppUser, UserResponseDto>> UserRepositoryMock = new();
    protected readonly Mock<IRepository<Post, PostResponseDto>> PostRepositoryMock = new();
    protected readonly Mock<IRepository<XComment, PostCommentResponseDto>> CommentRepositoryMock = new();
    protected readonly Mock<IRepository<XLike, PostLikeResponseDto>> LikeRepositoryMock = new();
}