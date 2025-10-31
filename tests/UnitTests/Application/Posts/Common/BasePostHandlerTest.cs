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
    protected readonly Mock<IRepository<AppUser>> UserEntityRepositoryMock = new();
    protected readonly Mock<IRepository<AppUser, UserResponseDto>> UserQueryRepositoryMock = new();
    
    protected readonly Mock<IRepository<Post>> PostEntityRepositoryMock = new();
    protected readonly Mock<IRepository<Post, PostResponseDto>> PostQueryRepositoryMock = new();
    
    protected readonly Mock<IRepository<XComment>> CommentEntityRepositoryMock = new();
    protected readonly Mock<IRepository<XComment, PostCommentResponseDto>> CommentQueryRepositoryMock = new();
    
    protected readonly Mock<IRepository<XLike>> LikeEntityRepositoryMock = new();
    protected readonly Mock<IRepository<XLike, PostLikeResponseDto>> LikeQueryRepositoryMock = new();
}