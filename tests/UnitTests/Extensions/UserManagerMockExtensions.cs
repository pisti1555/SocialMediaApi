using Infrastructure.Auth.Models;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace UnitTests.Extensions;

public static class UserManagerMockExtensions
{
    public static void SetupFindByIdAsync(this Mock<UserManager<AppIdentityUser>> userManagerMock,
        AppIdentityUser? user)
    {
        userManagerMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
    }
    
    public static void SetupCreateAsync(this Mock<UserManager<AppIdentityUser>> userManagerMock, IdentityResult creationResult, IdentityResult addToRoleResult)
    {
        userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<AppIdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(creationResult);
        
        userManagerMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<AppIdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(addToRoleResult);
    }
    
    public static void SetupCheckPasswordAsync(this Mock<UserManager<AppIdentityUser>> userManagerMock, bool result)
    {
        userManagerMock
            .Setup(x => x.CheckPasswordAsync(It.IsAny<AppIdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(result);
    }
}