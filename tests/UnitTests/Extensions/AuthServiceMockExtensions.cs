using Application;
using Application.Common.Results;
using Application.Contracts.Services;
using Domain.Users;
using Moq;

namespace UnitTests.Extensions;

public static class AuthServiceMockExtensions
{
    public static void SetupCreateIdentityUserFromAppUserAsync(this Mock<IAuthService> authServiceMock, IdentityUserCreationResult result)
    {
        authServiceMock
            .Setup(x => x.CreateIdentityUserFromAppUserAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }
    
    public static void SetupCheckPasswordAsync(this Mock<IAuthService> authServiceMock, bool result)
    {
        authServiceMock.Setup(x => x.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }
    
    public static void SetupGetRolesAsync(this Mock<IAuthService> authServiceMock, List<string> roles)
    {
        authServiceMock.Setup(x => x.GetRolesAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
    }
}