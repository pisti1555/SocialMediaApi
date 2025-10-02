using Domain.Users;
using Infrastructure.Auth.Exceptions;
using Infrastructure.Auth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Persistence.Auth.Models;
using UnitTests.Extensions;
using UnitTests.Factories;

namespace UnitTests.Infrastructure.Auth;

public class IdentityServiceTest
{
    private readonly Mock<UserManager<AppIdentityUser>> _userManagerMock;
    private readonly AppUser _appUser;
    private readonly AppIdentityUser _identityUser;
    
    private readonly IdentityService _identityService;

    public IdentityServiceTest()
    {
        var store = new Mock<IUserStore<AppIdentityUser>>();
        _userManagerMock = new Mock<UserManager<AppIdentityUser>>
        (
            store.Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<AppIdentityUser>>().Object, 
            Array.Empty<IUserValidator<AppIdentityUser>>(), 
            Array.Empty<IPasswordValidator<AppIdentityUser>>(),
            new Mock<ILookupNormalizer>().Object,
            new IdentityErrorDescriber(),
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<AppIdentityUser>>>().Object
        );
        
        _identityService = new IdentityService(_userManagerMock.Object);
        
        _appUser = TestDataFactory.CreateUser();
        _identityUser = new AppIdentityUser
        {
            Id = _appUser.Id,
            UserName = _appUser.UserName,
            Email = _appUser.Email
        };
    }

    // Check password
    [Fact]
    public async Task CheckPassword_WhenUsersPasswordMatchGivenPassword_ShouldReturnTrue()
    {
        _userManagerMock.SetupFindByIdAsync(_identityUser);
        _userManagerMock.SetupCheckPasswordAsync(true);

        var result = await _identityService.CheckPasswordAsync(_appUser, "Password");
        
        Assert.True(result);
    }
    
    [Fact]
    public async Task CheckPassword_WhenUserDoesNotExist_ShouldReturnFalse()
    {
        _userManagerMock.SetupFindByIdAsync(null);
        _userManagerMock.SetupCheckPasswordAsync(true);

        var result = await _identityService.CheckPasswordAsync(_appUser, "Password");
        
        Assert.False(result);
    }
    
    [Fact]
    public async Task CheckPassword_WhenUsersPasswordDoesNotMatchGivenPassword_ShouldReturnFalse()
    {
        _userManagerMock.SetupFindByIdAsync(_identityUser);
        _userManagerMock.SetupCheckPasswordAsync(false);
        
        var result = await _identityService.CheckPasswordAsync(_appUser, "Password");
        
        Assert.False(result);
    }

    // Create identity user from app user
    [Fact]
    public async Task CreateIdentityUserFromAppUserAsync_WhenValidRequest_ShouldReturnResult_WithSuccess()
    {
        _userManagerMock.SetupCreateAsync(IdentityResult.Success, IdentityResult.Success);
        
        var result = await _identityService.CreateIdentityUserFromAppUserAsync(_appUser, "Password");
        
        Assert.True(result.Succeeded);
        Assert.Empty(result.Errors);
    }
    
    [Fact]
    public async Task CreateIdentityUserFromAppUserAsync_WhenCreationFails_ShouldReturnFailedResult_WithErrors()
    {
        var creationErrors = new[]
        {
            new IdentityError { Code = "DuplicateUserName", Description = "Username already exists." },
            new IdentityError { Code = "InvalidEmail", Description = "Email is invalid." }
        };
        _userManagerMock.SetupCreateAsync(IdentityResult.Failed(creationErrors), IdentityResult.Success);
        
        var result = await _identityService.CreateIdentityUserFromAppUserAsync(_appUser, "Password");
        
        Assert.False(result.Succeeded);
        Assert.NotEmpty(result.Errors);
        Assert.Contains("Username already exists.", result.Errors);
        Assert.Contains("Email is invalid.", result.Errors);
    }
    
    [Fact]
    public async Task CreateIdentityUserFromAppUserAsync_WhenAddToRoleFails_ShouldReturnFailedResult_WithErrors()
    {
        var addToRoleErrors = new[]
        {
            new IdentityError { Code = "InvalidRoleName", Description = "Role does not exist." },
        };
        _userManagerMock.SetupCreateAsync(IdentityResult.Success, IdentityResult.Failed(addToRoleErrors));
        
        var result = await _identityService.CreateIdentityUserFromAppUserAsync(_appUser, "Password");
        
        Assert.False(result.Succeeded);
        Assert.NotEmpty(result.Errors);
        Assert.Contains("Role does not exist.", result.Errors);
    }
    
    [Fact]
    public async Task CreateIdentityUserFromAppUserAsync_WhenCreationAndAddToRoleFails_ShouldReturnFailedResult_WithErrorsOfBothFailedResults()
    {
        var creationErrors = new[]
        {
            new IdentityError { Code = "DuplicateUserName", Description = "Username already exists." },
            new IdentityError { Code = "InvalidEmail", Description = "Email is invalid." }
        };
        var addToRoleErrors = new[]
        {
            new IdentityError { Code = "InvalidRoleName", Description = "Role does not exist." },
        };
        _userManagerMock.SetupCreateAsync(IdentityResult.Failed(creationErrors), IdentityResult.Failed(addToRoleErrors));
        
        var result = await _identityService.CreateIdentityUserFromAppUserAsync(_appUser, "Password");
        
        Assert.False(result.Succeeded);
        Assert.NotEmpty(result.Errors);
        Assert.Contains("Username already exists.", result.Errors);
        Assert.Contains("Email is invalid.", result.Errors);
        Assert.Contains("Role does not exist.", result.Errors);
    }
    
    // Delete identity user
    [Fact]
    public async Task DeleteIdentityUserAsync_WhenIdentityUserExists_ShouldSucceed()
    {
        _userManagerMock.SetupFindByIdAsync(_identityUser);
        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<AppIdentityUser>()))
            .ReturnsAsync(new List<string> { "User" });
        _userManagerMock.Setup(x => x.RemoveFromRolesAsync(_identityUser, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.DeleteAsync(_identityUser))
            .ReturnsAsync(IdentityResult.Success);
        
        await _identityService.DeleteIdentityUserAsync(_appUser);
    }
    
    [Fact]
    public async Task DeleteIdentityUserAsync_WhenUserNotFound_ShouldThrowIdentityOperationException()
    {
        _userManagerMock.SetupFindByIdAsync(null);
        
        await Assert.ThrowsAsync<IdentityOperationException>(() => _identityService.DeleteIdentityUserAsync(_appUser));
    }
    
    [Fact]
    public async Task DeleteIdentityUserAsync_WhenRemoveFromRolesAsyncFails_ShouldThrowIdentityOperationException()
    {
        _userManagerMock.SetupFindByIdAsync(_identityUser);
        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<AppIdentityUser>()))
            .ReturnsAsync(["User"]);
        _userManagerMock.Setup(x => x.RemoveFromRolesAsync(_identityUser, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Failed());
        
        await Assert.ThrowsAsync<IdentityOperationException>(() => _identityService.DeleteIdentityUserAsync(_appUser));
    }
    
    [Fact]
    public async Task DeleteIdentityUserAsync_WhenDeleteAsyncFails_ShouldThrowIdentityOperationException()
    {
        _userManagerMock.SetupFindByIdAsync(_identityUser);
        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<AppIdentityUser>()))
            .ReturnsAsync(["User"]);
        _userManagerMock.Setup(x => x.RemoveFromRolesAsync(_identityUser, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.DeleteAsync(_identityUser))
            .ReturnsAsync(IdentityResult.Failed());
        
        await Assert.ThrowsAsync<IdentityOperationException>(() => _identityService.DeleteIdentityUserAsync(_appUser));
    }
    
    // Get roles
    [Fact]
    public async Task GetRolesAsync_WhenOk_ShouldReturnListOfRoles()
    {
        _userManagerMock.SetupFindByIdAsync(_identityUser);
        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<AppIdentityUser>()))
            .ReturnsAsync(["User"]);
        
        var result = await _identityService.GetRolesAsync(_appUser);

        Assert.NotEmpty(result);
    }
    
    [Fact]
    public async Task GetRolesAsync_WhenIdentityUserDoesNotExist_ShouldReturnEmptyList()
    {
        _userManagerMock.SetupFindByIdAsync(null);
        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<AppIdentityUser>()))
            .ReturnsAsync([]);
        
        var result = await _identityService.GetRolesAsync(_appUser);

        Assert.Empty(result);
    }
}