using System.Linq.Expressions;
using Application.Common.Pagination;
using Application.Contracts.Persistence.Repositories;
using Application.Responses;
using AutoMapper;
using Domain.Users;
using Moq;

namespace UnitTests.Extensions;

public static class AppUserRepositoryMockExtensions
{
    public static void SetupUser(this Mock<IRepository<AppUser>> userRepositoryMock, AppUser? user)
    {
        userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<Guid>(id => user != null && id == user.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
    }
    
    public static void SetupUser(this Mock<IRepository<AppUser, UserResponseDto>> userRepositoryMock, AppUser? user, IMapper mapper)
    {
        userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<Guid>(id => user != null && id == user.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user is not null ? mapper.Map<UserResponseDto>(user) : null);
    }

    public static void SetupGetPaged(this Mock<IRepository<AppUser, UserResponseDto>> userRepositoryMock, PagedResult<UserResponseDto> users)
    {
        userRepositoryMock
            .Setup(x => x.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<AppUser, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);
    }
    
    public static void SetupUserExists(this Mock<IRepository<AppUser>> userRepositoryMock, Guid userId, bool exists)
    {
        userRepositoryMock
            .Setup(x => x.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }
    
    public static void SetupUserExists(this Mock<IRepository<AppUser, UserResponseDto>> userRepositoryMock, Guid userId, bool exists)
    {
        userRepositoryMock
            .Setup(x => x.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }
    
    public static void SetupUserExistsByAnyFilters(this Mock<IRepository<AppUser>> userRepositoryMock, bool exists)
    {
        userRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }
    
    public static void SetupUserExistsByAnyFilters(this Mock<IRepository<AppUser, UserResponseDto>> userRepositoryMock, bool exists)
    {
        userRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }
    
    public static void SetupSaveChanges(this Mock<IRepository<AppUser>> userRepositoryMock, bool success = true)
    {
        userRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(success);
    }
    
    public static void VerifyGetPaged(this Mock<IRepository<AppUser, UserResponseDto>> userRepositoryMock, bool called = true)
    {
        userRepositoryMock
            .Verify(x => 
                    x.GetPagedAsync(
                        It.IsAny<int>(), 
                        It.IsAny<int>(), 
                        It.IsAny<Expression<Func<AppUser, bool>>>(), 
                        It.IsAny<CancellationToken>()
                    ), called ? Times.Once : Times.Never
            );
    }
}