using System.Linq.Expressions;
using Application.Contracts.Services;
using Application.Requests.Users.Root.Commands.Login;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;
using Moq;
using UnitTests.Application.Users.Common;
using UnitTests.Extensions;
using UnitTests.Factories;

namespace UnitTests.Application.Users.Root.Commands;

public class LoginHandlerTest : BaseUserHandlerTest
{
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    
    private readonly LoginHandler _loginHandler;

    public LoginHandlerTest()
    {
        _loginHandler = new LoginHandler(
            UserRepositoryMock.Object,
            AuthServiceMock.Object,
            _tokenServiceMock.Object,
            Mapper
        );
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldReturnUserDto_WithJwtTokenIncluded()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        
        var command = new LoginCommand(user.UserName, "Test-Password-123");
        
        UserRepositoryMock.Setup(x => x.GetEntityAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        AuthServiceMock.SetupCheckPasswordAsync(true);
        AuthServiceMock.SetupGetRolesAsync(["User"]);
        _tokenServiceMock.SetupCreateToken();

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
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        
        UserRepositoryMock.Verify(x => x.GetEntityAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        AuthServiceMock.Verify(x => x.CheckPasswordAsync(user, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        AuthServiceMock.Verify(x => x.GetRolesAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _tokenServiceMock.Verify(x => x.CreateToken(user, new[]{"User"}), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var command = new LoginCommand("validUser", "Test-Password-123");
        
        UserRepositoryMock.Setup(x => x.GetEntityAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppUser?)null);
        AuthServiceMock.SetupCheckPasswordAsync(true);
        AuthServiceMock.SetupGetRolesAsync(["User"]);
        _tokenServiceMock.SetupCreateToken();

        // Act
        await Assert.ThrowsAsync<UnauthorizedException>(() => _loginHandler.Handle(command, CancellationToken.None));
        
        // Assert
        UserRepositoryMock.Verify(x => x.GetEntityAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        AuthServiceMock.Verify(x => x.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        AuthServiceMock.Verify(x => x.GetRolesAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenServiceMock.Verify(x => x.CreateToken(It.IsAny<AppUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenPasswordIsInvalid_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        var command = new LoginCommand("validUser", "Test-Password-123");
        
        UserRepositoryMock.Setup(x => x.GetEntityAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        AuthServiceMock.SetupCheckPasswordAsync(false);
        AuthServiceMock.SetupGetRolesAsync(["User"]);
        _tokenServiceMock.SetupCreateToken();

        // Act
        await Assert.ThrowsAsync<UnauthorizedException>(() => _loginHandler.Handle(command, CancellationToken.None));
        
        // Assert
        UserRepositoryMock.Verify(x => x.GetEntityAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        AuthServiceMock.Verify(x => x.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        AuthServiceMock.Verify(x => x.GetRolesAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenServiceMock.Verify(x => x.CreateToken(It.IsAny<AppUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }
}