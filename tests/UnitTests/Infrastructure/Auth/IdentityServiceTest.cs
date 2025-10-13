using System.Linq.Expressions;
using System.Security.Claims;
using Application.Contracts.Auth;
using Application.Contracts.Services;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;
using Infrastructure.Auth.Exceptions;
using Infrastructure.Auth.Models;
using Infrastructure.Auth.Services;
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
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IHasher> _hasherMock;
    
    private readonly AppUser _appUser;
    private readonly AppIdentityUser _identityUser;
    
    private readonly IdentityService _identityService;

    public IdentityServiceTest()
    {
        _tokenRepositoryMock = new Mock<IOutsideServicesRepository<Token>>();
        _tokenServiceMock = new Mock<ITokenService>();
        _hasherMock = new Mock<IHasher>();
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
            _tokenServiceMock.Object, 
            _tokenRepositoryMock.Object, 
            _hasherMock.Object,
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
        var userId = Guid.NewGuid();
        var claims = new List<Claim>()
        {
            new(TokenClaims.TokenId, Guid.NewGuid().ToString("N")),
            new(TokenClaims.SessionId, Guid.NewGuid().ToString("N")),
            new(TokenClaims.UserId, userId.ToString()),
        };

        _tokenServiceMock.Setup(x => x.GetClaimsFromToken(It.IsAny<string>()))
            .Returns(claims);
        _hasherMock.Setup(x => x.CreateHash(It.IsAny<string>()))
            .Returns("hashed-value");

        await _identityService.SaveTokenAsync("test-access-token", "test-refresh-token", false);

        _tokenServiceMock.Verify(x => x.GetClaimsFromToken(It.IsAny<string>()), Times.Once);
        _hasherMock.Verify(x => x.CreateHash(It.IsAny<string>()), Times.Exactly(2));
    }
    
    [Fact]
    public async Task SaveTokenAsync_WhenSidIsMissing_ShouldThrowIdentityOperationException()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(TokenClaims.UserId, Guid.NewGuid().ToString())
        };

        _tokenServiceMock.Setup(x => x.GetClaimsFromToken(It.IsAny<string>()))
            .Returns(claims);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _identityService.SaveTokenAsync("test-access-token", "test-refresh-token", false));
    }
    
    [Fact]
    public async Task SaveTokenAsync_WhenSidIsInvalid_ShouldThrowIdentityOperationException()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(TokenClaims.SessionId, "invalid-session-id"),
            new(TokenClaims.UserId, Guid.NewGuid().ToString())
        };

        _tokenServiceMock.Setup(x => x.GetClaimsFromToken(It.IsAny<string>()))
            .Returns(claims);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _identityService.SaveTokenAsync("test-access-token", "test-refresh-token", false));
    }
    
        
    // Update token
    [Fact]
    public async Task UpdateTokenAsync_WhenValid_ShouldUpdateToken()
    {
        // Arrange
        var originalToken = TestDataFactory.CreateToken();
        
        var oldJti = Guid.NewGuid().ToString("N");
        var newJti = Guid.NewGuid().ToString("N");
        var newJtiHash = "hashed-new-jti";
        var oldRefreshToken = "old-refresh-token";
        var newRefreshToken = "new-refresh-token";
        var newRefreshTokenHash = "hashed-new-refresh-token";
        
        _hasherMock.Setup(x => x.CreateHash(oldRefreshToken)).Returns(originalToken.RefreshTokenHash);
        _hasherMock.Setup(x => x.CreateHash(oldJti)).Returns(originalToken.JtiHash);
        
        _hasherMock.Setup(x => x.CreateHash(newRefreshToken)).Returns(newRefreshTokenHash);
        _hasherMock.Setup(x => x.CreateHash(newJti)).Returns(newJtiHash);
        
        _tokenRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Token, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalToken);

        // Act
        await _identityService.UpdateTokenAsync(
            oldRefreshToken, 
            newRefreshToken, 
            originalToken.Id, 
            originalToken.UserId.ToString(), 
            oldJti, 
            newJti, 
            CancellationToken.None
        );

        // Assert
        _hasherMock.Verify(x => x.CreateHash(It.IsAny<string>()), Times.Exactly(4));
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
    public async Task UpdateTokenAsync_WhenMissingRequiredField_ShouldThrowUnauthorizedException(
        string? refreshToken, string newRefreshToken, string? sid, string? uid, string? jti, string? newJti)
    {
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _identityService.UpdateTokenAsync(refreshToken, newRefreshToken, sid, uid, jti, newJti)
        );
    }
    
    [Fact]
    public async Task UpdateTokenAsync_WhenTokenCannotBeFoundBySessionID_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var originalToken = TestDataFactory.CreateToken();
        
        var oldJti = Guid.NewGuid().ToString("N");
        var newJti = Guid.NewGuid().ToString("N");
        var newJtiHash = "hashed-new-jti";
        var oldRefreshToken = "old-refresh-token";
        var newRefreshToken = "new-refresh-token";
        var newRefreshTokenHash = "hashed-new-refresh-token";
        
        _hasherMock.Setup(x => x.CreateHash(oldRefreshToken)).Returns(originalToken.RefreshTokenHash);
        _hasherMock.Setup(x => x.CreateHash(oldJti)).Returns(originalToken.JtiHash);
        
        _hasherMock.Setup(x => x.CreateHash(newRefreshToken)).Returns(newRefreshTokenHash);
        _hasherMock.Setup(x => x.CreateHash(newJti)).Returns(newJtiHash);
        
        _tokenRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Token, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Token?)null);
        
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _identityService.UpdateTokenAsync(oldRefreshToken, newRefreshToken, "will-not-find-token-by-sid", originalToken.UserId.ToString(), oldJti, newJti)
        );
    }
    
    [Theory]
    [InlineData("different-user-id", null, null)]
    [InlineData(null, "different-jti", null)]
    [InlineData(null, null, "different-refresh-token")]
    public async Task UpdateTokenAsync_WhenTokenDataDoesNotMatchWithProvidedData_ShouldDeleteToken_ThenThrowUnauthorizedException(
        string? differentUserId, string? differentJti, string? differentRefreshToken
    )
    {
        // Arrange
        var originalToken = TestDataFactory.CreateToken();
        
        var oldJti = Guid.NewGuid().ToString("N");
        var newJti = Guid.NewGuid().ToString("N");
        var newJtiHash = "hashed-new-jti";
        var oldRefreshToken = "old-refresh-token";
        var newRefreshToken = "new-refresh-token";
        var newRefreshTokenHash = "hashed-new-refresh-token";
        
        _hasherMock.Setup(x => x.CreateHash(oldRefreshToken)).Returns(originalToken.RefreshTokenHash);
        _hasherMock.Setup(x => x.CreateHash(oldJti)).Returns(originalToken.JtiHash);

        if (!string.IsNullOrEmpty(differentJti))
        {
            _hasherMock.Setup(x => x.CreateHash(differentJti)).Returns("different-jti-hash");
        }

        if (!string.IsNullOrEmpty(differentRefreshToken))
        {
            _hasherMock.Setup(x => x.CreateHash(differentRefreshToken)).Returns("different-refresh-token-hash");
        }
        
        _hasherMock.Setup(x => x.CreateHash(newRefreshToken)).Returns(newRefreshTokenHash);
        _hasherMock.Setup(x => x.CreateHash(newJti)).Returns(newJtiHash);
        
        _tokenRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Token, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalToken);
        
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _identityService.UpdateTokenAsync(
                differentRefreshToken ?? oldRefreshToken, 
                newRefreshToken, 
                originalToken.Id, 
                differentUserId ?? originalToken.UserId.ToString(), 
                differentJti ?? oldJti, 
                newJti
            )
        );
        
        _tokenRepositoryMock.Verify(x => x.Delete(originalToken), Times.Once);
        _tokenRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task UpdateTokenAsync_WhenTokenExpired_ShouldDeleteToken_ThenThrowUnauthorizedException()
    {
        // Arrange
        var sid = Guid.NewGuid().ToString("N");
        var uid = Guid.NewGuid();

        var token = TestDataFactory.CreateToken(isExpired: true);

        _hasherMock.Setup(x => x.CreateHash(It.IsAny<string>())).Returns("hashed-value");
        _tokenRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Token, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _identityService.UpdateTokenAsync("old-refresh", "new-refresh", sid, uid.ToString(), "old-jti", "new-jti")
        );

        _tokenRepositoryMock.Verify(x => x.Delete(token), Times.Once);
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