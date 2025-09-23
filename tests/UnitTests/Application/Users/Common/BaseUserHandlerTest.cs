using Application.Contracts.Persistence.Repositories;
using Application.Responses;
using Domain.Users;
using Moq;
using UnitTests.Common;

namespace UnitTests.Application.Users.Common;

public abstract class BaseUserHandlerTest : TestBase
{
    protected readonly Mock<IRepository<AppUser, UserResponseDto>> UserRepositoryMock = new();
}