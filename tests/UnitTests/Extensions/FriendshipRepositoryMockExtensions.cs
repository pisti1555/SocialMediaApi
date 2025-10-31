using System.Linq.Expressions;
using Application.Common.Pagination;
using Application.Contracts.Persistence.Repositories;
using Application.Responses;
using AutoMapper;
using Domain.Users;
using Moq;

namespace UnitTests.Extensions;

public static class FriendshipRepositoryMockExtensions
{
    public static void SetupFriendship(this Mock<IRepository<Friendship>> repositoryMock, Friendship? friendship)
    {
        repositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<Guid>(id => friendship != null && id == friendship.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(friendship);
    }
    
    public static void SetupFriendship(this Mock<IRepository<Friendship, FriendshipResponseDto>> repositoryMock, Friendship? friendship, IMapper mapper)
    {
        repositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<Guid>(id => friendship != null && id == friendship.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(friendship is not null ? mapper.Map<FriendshipResponseDto>(friendship) : null);
    }
    
    public static void SetupGetAll(this Mock<IRepository<Friendship>> repositoryMock, List<Friendship> friendships)
    {
        repositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Friendship, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(friendships);
    }
    
    public static void SetupGetAll(this Mock<IRepository<Friendship, FriendshipResponseDto>> repositoryMock, List<FriendshipResponseDto> friendships)
    {
        repositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Friendship, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(friendships);
    }

    public static void SetupGetPaged(this Mock<IRepository<Friendship, FriendshipResponseDto>> repositoryMock, PagedResult<FriendshipResponseDto> friendships)
    {
        repositoryMock
            .Setup(x => x.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Friendship, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(friendships);
    }
    
    public static void SetupFriendshipExists(this Mock<IRepository<Friendship>> repositoryMock, Guid friendshipId, bool exists)
    {
        repositoryMock
            .Setup(x => x.ExistsAsync(friendshipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }
    
    public static void SetupFriendshipExists(this Mock<IRepository<Friendship, FriendshipResponseDto>> repositoryMock, Guid friendshipId, bool exists)
    {
        repositoryMock
            .Setup(x => x.ExistsAsync(friendshipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }
    
    public static void SetupFriendshipExistsByAnyFilters(this Mock<IRepository<Friendship>> repositoryMock, bool exists)
    {
        repositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<Friendship, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }
    
    public static void SetupFriendshipExistsByAnyFilters(this Mock<IRepository<Friendship, FriendshipResponseDto>> repositoryMock, bool exists)
    {
        repositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<Friendship, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }
    
    public static void SetupSaveChanges(this Mock<IRepository<Friendship>> repositoryMock, bool success = true)
    {
        repositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(success);
    }
}