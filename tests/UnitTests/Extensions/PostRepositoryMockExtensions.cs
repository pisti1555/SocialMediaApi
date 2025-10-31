using System.Linq.Expressions;
using Application.Common.Pagination;
using Application.Contracts.Persistence.Repositories;
using Application.Responses;
using AutoMapper;
using Domain.Posts;
using Moq;

namespace UnitTests.Extensions;

public static class PostRepositoryMockExtensions
{
    public static void SetupPost(this Mock<IRepository<Post>> postRepositoryMock, Post? post)
    {
        postRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<Guid>(id => post != null && id == post.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);
    }
    
    public static void SetupPost(this Mock<IRepository<Post, PostResponseDto>> postRepositoryMock, Post? post, IMapper mapper)
    {
        postRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<Guid>(id => post != null && id == post.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(post is not null ? mapper.Map<PostResponseDto>(post) : null);
    }
    
    public static void SetupGetPaged(this Mock<IRepository<Post, PostResponseDto>> postRepositoryMock, PagedResult<PostResponseDto> posts)
    {
        postRepositoryMock
            .Setup(x => x.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Post, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(posts);
    }
    
    public static void SetupPostExists(this Mock<IRepository<Post>> postRepositoryMock, Guid postId, bool exists)
    {
        postRepositoryMock
            .Setup(x => x.ExistsAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }
    
    public static void SetupPostExists(this Mock<IRepository<Post, PostResponseDto>> postRepositoryMock, Guid postId, bool exists)
    {
        postRepositoryMock
            .Setup(x => x.ExistsAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }
    
    public static void SetupPostExistsByAnyFilters(this Mock<IRepository<Post>> userRepositoryMock, bool exists)
    {
        userRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<Post, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }
    
    public static void SetupPostExistsByAnyFilters(this Mock<IRepository<Post, PostResponseDto>> userRepositoryMock, bool exists)
    {
        userRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<Post, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    public static void SetupSaveChanges(this Mock<IRepository<Post>> postRepositoryMock, bool success = true)
    {
        postRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(success);
    }
    
    public static void VerifyGetPaged(this Mock<IRepository<Post, PostResponseDto>> postRepositoryMock, bool called = true)
    {
        postRepositoryMock
            .Verify(x => 
                    x.GetPagedAsync(
                        It.IsAny<int>(), 
                        It.IsAny<int>(), 
                        It.IsAny<Expression<Func<Post, bool>>>(), 
                        It.IsAny<CancellationToken>()
                    ), called ? Times.Once : Times.Never
            );
    }
}