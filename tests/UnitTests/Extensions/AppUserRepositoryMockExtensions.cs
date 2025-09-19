using Application.Contracts.Persistence.Repositories.AppUser;
using Domain.Users;
using Moq;

namespace UnitTests.Extensions;

public static class AppUserRepositoryMockExtensions
{
    public static void SetupUser(this Mock<IAppUserRepository> userRepositoryMock, AppUser? user) =>
        userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<Guid>(id => user != null && id == user.Id)))
            .ReturnsAsync(user);
}