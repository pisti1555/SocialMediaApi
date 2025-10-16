using Application.Common.Results;
using Application.Contracts.Services;
using Domain.Users;
using Moq;

namespace UnitTests.Extensions;

public static class AuthServiceMockExtensions
{
    public static void SetupCreateIdentityUserAsync(this Mock<IAuthService> authServiceMock, AppResult result)
    {
        authServiceMock
            .Setup(x => x.CreateIdentityUserAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
    
    public static void SetupSaveTokenAsync(this Mock<IAuthService> authServiceMock, AppResult result)
    {
        authServiceMock.Setup(x => x.SaveTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }
    
    public static void SetupUpdateTokenAsync(this Mock<IAuthService> authServiceMock, AppResult result)
    {
        authServiceMock.Setup(x => x.UpdateTokenAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }
}