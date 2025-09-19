using Application.Contracts.Persistence.Repositories.AppUser;
using Moq;
using UnitTests.Common;

namespace UnitTests.Application.Users.Common;

public abstract class BaseUserHandlerTest : TestBase
{
    protected readonly Mock<IAppUserRepository> UserRepositoryMock = new();
}