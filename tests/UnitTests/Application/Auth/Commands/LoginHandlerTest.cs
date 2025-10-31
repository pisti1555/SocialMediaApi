using System.Linq.Expressions;
using Application.Common.Adapters.Auth;
using Application.Common.Results;
using Application.Contracts.Services;
using Application.Requests.Auth.Commands.Login;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;
using Moq;
using UnitTests.Application.Users.Common;
using UnitTests.Extensions;
using UnitTests.Factories;

namespace UnitTests.Application.Auth.Commands;

public class LoginHandlerTest : BaseUserHandlerTest
{
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IHasher> _hasherMock = new();
    
    private readonly LoginHandler _loginHandler;

    private readonly AccessTokenClaims _claims;

    public LoginHandlerTest()
    {
        _loginHandler = new LoginHandler(
            UserEntityRepositoryMock.Object,
            AuthServiceMock.Object,
            _tokenServiceMock.Object,
            _hasherMock.Object,
            Mapper
        );
        
        _claims = TestDataFactory.CreateAccessTokenClaims();
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldReturnUserDto_WithTokensIncluded()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        
        var command = new LoginCommand(user.UserName, "Test-Password-123", false);
        
        UserEntityRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        AuthServiceMock.SetupCheckPasswordAsync(true);
        AuthServiceMock.SetupGetRolesAsync(["User"]);
        AuthServiceMock.SetupSaveTokenAsync(AppResult.Success());
        _tokenServiceMock.SetupCreateAccessToken();
        _tokenServiceMock.SetupCreateRefreshToken();
        _tokenServiceMock.SetupGetValidatedClaimsFromToken(AppResult<AccessTokenClaims?>.Success(_claims));
        _hasherMock.Setup(x => x.CreateHash(It.IsAny<string>())).Returns("hashed-value");

        // Act
        var result = await _loginHandler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.UserName, result.UserName);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.FirstName, result.FirstName);
        Assert.Equal(user.LastName, result.LastName);
        Assert.Equal(user.DateOfBirth, result.DateOfBirth);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        
        UserEntityRepositoryMock.Verify(x => x.GetAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        AuthServiceMock.Verify(x => x.CheckPasswordAsync(user, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _tokenServiceMock.Verify(x => x.CreateAccessToken(user.Id.ToString(), user.UserName, user.Email, new[]{"User"}, null), Times.Once);
        _tokenServiceMock.Verify(x => x.CreateRefreshToken(), Times.Once);
        AuthServiceMock.Verify(x => x.SaveTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var command = new LoginCommand("validUser", "Test-Password-123", false);
        
        UserEntityRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppUser?)null);
        AuthServiceMock.SetupCheckPasswordAsync(true);
        AuthServiceMock.SetupGetRolesAsync(["User"]);
        AuthServiceMock.SetupSaveTokenAsync(AppResult.Success());
        _tokenServiceMock.SetupCreateAccessToken();
        _tokenServiceMock.SetupCreateRefreshToken();
        _tokenServiceMock.SetupGetValidatedClaimsFromToken(AppResult<AccessTokenClaims?>.Success(_claims));
        _hasherMock.Setup(x => x.CreateHash(It.IsAny<string>())).Returns("hashed-value");

        // Act
        await Assert.ThrowsAsync<UnauthorizedException>(() => _loginHandler.Handle(command, CancellationToken.None));
        
        // Assert
        UserEntityRepositoryMock.Verify(x => x.GetAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        AuthServiceMock.Verify(x => x.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        AuthServiceMock.Verify(x => x.SaveTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenServiceMock.Verify(x => x.CreateAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>()), Times.Never);
        _tokenServiceMock.Verify(x => x.CreateRefreshToken(), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenPasswordIsInvalid_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var command = new LoginCommand("validUser", "Test-Password-123", false);
        
        UserEntityRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        AuthServiceMock.SetupCheckPasswordAsync(false);
        AuthServiceMock.SetupGetRolesAsync(["User"]);
        AuthServiceMock.SetupSaveTokenAsync(AppResult.Success());
        _tokenServiceMock.SetupCreateAccessToken();
        _tokenServiceMock.SetupCreateRefreshToken();
        _tokenServiceMock.SetupGetValidatedClaimsFromToken(AppResult<AccessTokenClaims?>.Success(_claims));
        _hasherMock.Setup(x => x.CreateHash(It.IsAny<string>())).Returns("hashed-value");

        // Act
        await Assert.ThrowsAsync<UnauthorizedException>(() => _loginHandler.Handle(command, CancellationToken.None));
        
        // Assert
        UserEntityRepositoryMock.Verify(x => x.GetAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        AuthServiceMock.Verify(x => x.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        AuthServiceMock.Verify(x => x.SaveTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenServiceMock.Verify(x => x.CreateAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>()), Times.Never);
        _tokenServiceMock.Verify(x => x.CreateRefreshToken(), Times.Never);
    }
}