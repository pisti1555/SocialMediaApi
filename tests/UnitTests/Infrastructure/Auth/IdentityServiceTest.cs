using System.Linq.Expressions;
using Domain.Users;
using Infrastructure.Auth.Models;
using Infrastructure.Auth.Services;
using Infrastructure.Common.Exceptions;
using Infrastructure.Persistence.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UnitTests.Extensions;
using UnitTests.Factories;

namespace UnitTests.Infrastructure.Auth;

public class IdentityServiceTest
{
    private readonly Mock<UserManager<AppIdentityUser>> _userManagerMock;
    private readonly Mock<IOutsideServicesRepository<Token>> _tokenRepositoryMock;
    
    private readonly AppUser _appUser;
    private readonly AppIdentityUser _identityUser;
    
    private readonly IdentityService _identityService;

    public IdentityServiceTest()
    {
        _tokenRepositoryMock = new Mock<IOutsideServicesRepository<Token>>();
        var loggerMock = new Mock<ILogger<IdentityService>>();
        
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
        
        _identityService = new IdentityService(
            _userManagerMock.Object, 
            _tokenRepositoryMock.Object, 
            loggerMock.Object
        );
        
        _appUser = TestDataFactory.CreateUser();
        _identityUser = new AppIdentityUser
        {
            Id = _appUser.Id,
            UserName = _appUser.UserName,
            Email = _appUser.Email
        };
    }
    
    // Save token
    [Fact]
    public async Task SaveTokenAsync_WhenValidRequest_ShouldCreateTokenEntity_ThenSaveItToDbContext()
    {
        var result = await _identityService.SaveTokenAsync(
            "access-token-hash", 
            "refresh-token-hash", 
            "test-sid", 
            Guid.NewGuid().ToString(),  
            false
        );
        
        Assert.True(result.Succeeded);
        Assert.Empty(result.Errors);

        _tokenRepositoryMock.Verify(x => x.Add(It.IsAny<Token>()), Times.Once);
        _tokenRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task SaveTokenAsync_WhenCannotParseUserId_ShouldReturnFailure()
    {
        // Act & Assert
        var result = await _identityService.SaveTokenAsync(
            "access-token-hash", 
            "refresh-token-hash", 
            "test-sid", 
            "test-uid", 
            false
        );
        
        Assert.False(result.Succeeded);
        Assert.NotEmpty(result.Errors);
    }
        
    // Update token
    [Fact]
    public async Task UpdateTokenAsync_WhenValid_ShouldUpdateToken()
    {
        // Arrange
        var originalToken = TestDataFactory.CreateToken();
        
        var newJtiHash = "hashed-new-jti";
        var newRefreshTokenHash = "hashed-new-refresh-token";
        
        _tokenRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Token, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalToken);

        // Act
        var result = await _identityService.UpdateTokenAsync(
            oldRefreshTokenHash: originalToken.RefreshTokenHash, 
            oldJtiHash: originalToken.JtiHash, 
            sid: originalToken.Id, 
            uid: originalToken.UserId.ToString(), 
            newRefreshTokenHash: newRefreshTokenHash, 
            newJtiHash: newJtiHash, 
            ct: CancellationToken.None
        );

        // Assert
        Assert.True(result.Succeeded);
        Assert.Empty(result.Errors);
        
        _tokenRepositoryMock.Verify(x => x.GetAsync(It.IsAny<Expression<Func<Token, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        _tokenRepositoryMock.Verify(x => x.Update(It.IsAny<Token>()), Times.Once);
        _tokenRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Theory]
    [InlineData(null, "new-refresh-token", "sid", "uid", "jti", "new-jti")]
    [InlineData("", "new-refresh-token", "sid", "uid", "jti", "new-jti")]
    [InlineData("refresh-token", "new-refresh-token", null, "uid", "jti", "new-jti")]
    [InlineData("refresh-token", "new-refresh-token", "sid", null, "jti", "new-jti")]
    [InlineData("refresh-token", "new-refresh-token", "sid", "uid", null, "new-jti")]
    [InlineData("refresh-token", "new-refresh-token", "sid", "uid", "jti", null)]
    public async Task UpdateTokenAsync_WhenMissingRequiredField_ShouldReturnFailure(
        string? refreshToken, string newRefreshToken, string? sid, string? uid, string? jti, string? newJti)
    {
        // Act & Assert
        await _identityService.UpdateTokenAsync(refreshToken, newRefreshToken, sid, uid, jti, newJti);
    }
    
    [Fact]
    public async Task UpdateTokenAsync_WhenTokenCannotBeFoundBySessionID_ShouldReturnFailure()
    {
        // Arrange
        var originalToken = TestDataFactory.CreateToken();
        
        var newJtiHash = "hashed-new-jti";
        var newRefreshTokenHash = "hashed-new-refresh-token";
        
        _tokenRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Token, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Token?)null);
        
        // Act
        var result = await _identityService.UpdateTokenAsync(
            oldRefreshTokenHash: originalToken.RefreshTokenHash, 
            oldJtiHash: originalToken.JtiHash, 
            sid: originalToken.Id, 
            uid: originalToken.UserId.ToString(), 
            newRefreshTokenHash: newRefreshTokenHash, 
            newJtiHash: newJtiHash, 
            ct: CancellationToken.None
        );
        
        // Assert
        Assert.False(result.Succeeded);
        Assert.NotEmpty(result.Errors);
    }
    
    [Theory]
    [InlineData("different-user-id", null, null)]
    [InlineData(null, "different-jti", null)]
    [InlineData(null, null, "different-refresh-token")]
    public async Task UpdateTokenAsync_WhenTokenDataDoesNotMatchWithProvidedData_ShouldDeleteToken_ThenReturnFailure(
        string? differentUserId, string? differentJtiHash, string? differentRefreshTokenHash
    )
    {
        // Arrange
        var originalToken = TestDataFactory.CreateToken();
        
        var newJti = Guid.NewGuid().ToString("N");
        var newRefreshTokenHash = "hashed-new-refresh-token";
        
        _tokenRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Token, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalToken);
        
        // Act
        var result = await _identityService.UpdateTokenAsync(
            oldRefreshTokenHash: differentRefreshTokenHash ?? originalToken.RefreshTokenHash,  
            oldJtiHash: differentJtiHash ?? originalToken.JtiHash,  
            sid: originalToken.Id, 
            uid: differentUserId ?? originalToken.UserId.ToString(), 
            newRefreshTokenHash: newRefreshTokenHash,
            newJtiHash: newJti,
            CancellationToken.None
        );
        
        // Assert
        Assert.False(result.Succeeded);
        Assert.NotEmpty(result.Errors);
        
        _tokenRepositoryMock.Verify(x => x.Delete(originalToken), Times.Once);
        _tokenRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task UpdateTokenAsync_WhenTokenExpired_ShouldDeleteToken_ThenReturnFailure()
    {
        // Arrange
        var originalToken = TestDataFactory.CreateToken(isExpired: true);
        
        _tokenRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Token, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalToken);

        // Act
        var result = await _identityService.UpdateTokenAsync(
            oldRefreshTokenHash: originalToken.RefreshTokenHash, 
            oldJtiHash: originalToken.JtiHash, 
            sid: originalToken.Id, 
            uid: originalToken.UserId.ToString(), 
            newRefreshTokenHash: "new-refresh-token-hash",  
            newJtiHash: "new-jti-hash", 
            ct: CancellationToken.None
        );

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotEmpty(result.Errors);
        
        _tokenRepositoryMock.Verify(x => x.Delete(originalToken), Times.Once);
        _tokenRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _tokenRepositoryMock.Verify(x => x.Update(It.IsAny<Token>()), Times.Never);
    }

    // Check password
    [Fact]
    public async Task CheckPasswordAsync_WhenUsersPasswordMatchGivenPassword_ShouldReturnTrue()
    {
        _userManagerMock.SetupFindByIdAsync(_identityUser);
        _userManagerMock.SetupCheckPasswordAsync(true);

        var result = await _identityService.CheckPasswordAsync(_appUser, "Password");
        
        Assert.True(result);
    }
    
    [Fact]
    public async Task CheckPasswordAsync_WhenUserDoesNotExist_ShouldReturnFalse()
    {
        _userManagerMock.SetupFindByIdAsync(null);
        _userManagerMock.SetupCheckPasswordAsync(true);

        var result = await _identityService.CheckPasswordAsync(_appUser, "Password");
        
        Assert.False(result);
    }
    
    [Fact]
    public async Task CheckPasswordAsync_WhenUsersPasswordDoesNotMatchGivenPassword_ShouldReturnFalse()
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
        
        var result = await _identityService.CreateIdentityUserAsync(_appUser, "Password");
        
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
        
        var result = await _identityService.CreateIdentityUserAsync(_appUser, "Password");
        
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
        
        var result = await _identityService.CreateIdentityUserAsync(_appUser, "Password");
        
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
        
        var result = await _identityService.CreateIdentityUserAsync(_appUser, "Password");
        
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