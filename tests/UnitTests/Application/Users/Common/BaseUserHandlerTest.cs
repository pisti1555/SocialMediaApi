using Application.Contracts.Persistence.Repositories;
using Application.Responses;
using Domain.Users;
using Moq;
using UnitTests.Common;

namespace UnitTests.Application.Users.Common;

public abstract class BaseUserHandlerTest : TestBase
{
    protected readonly Mock<IRepository<AppUser>> UserEntityRepositoryMock = new();
    protected readonly Mock<IRepository<AppUser, UserResponseDto>> UserQueryRepositoryMock = new();
    
    protected readonly Mock<IRepository<Friendship>> FriendshipEntityRepositoryMock = new();
    protected readonly Mock<IRepository<Friendship, FriendshipResponseDto>> FriendshipQueryRepositoryMock = new();
}