using System.Security.Claims;
using Application.Contracts.Services;
using Application.Requests.Auth.Commands.RefreshAccess;
using Domain.Common.Exceptions.CustomExceptions;
using Moq;
using UnitTests.Application.Users.Common;
using UnitTests.Extensions;

namespace UnitTests.Application.Auth.Commands;

public class RefreshAccessHandlerTest : BaseUserHandlerTest
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();

    private readonly List<Claim> _claims;
    
    private readonly RefreshAccessHandler _refreshAccessHandler;

    public RefreshAccessHandlerTest()
    {
        _claims = []; // Validated in ITokenService, so no need to fill it, just mock the returned value in tests.
        _refreshAccessHandler = new RefreshAccessHandler(
            _authServiceMock.Object, 
            _tokenServiceMock.Object
        );
    }
    
    [Fact]
    public async Task Handle_WhenValidRequest_ShouldReturnTokenResponseDto()
    {
        // Arrange
        _tokenServiceMock.SetupGetClaimsFromToken(_claims);
        _tokenServiceMock.SetupValidateToken(true);
        _tokenServiceMock.SetupCreateAccessToken("new-access-token");
        _tokenServiceMock.SetupCreateRefreshToken("new-refresh-token");
        
        var command = new RefreshAccessCommand("old-access-token", "old-refresh-token");
        
        // Act
        var result = await _refreshAccessHandler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("new-access-token", result.AccessToken);
        Assert.Equal("new-refresh-token", result.RefreshToken);
    }
    
    [Fact]
    public async Task Handle_WhenInvalidClaims_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _tokenServiceMock.SetupGetClaimsFromToken(_claims);
        _tokenServiceMock.SetupValidateToken(false);
        _tokenServiceMock.SetupCreateAccessToken("new-access-token");
        _tokenServiceMock.SetupCreateRefreshToken("new-refresh-token");
        
        var command = new RefreshAccessCommand("old-access-token", "old-refresh-token");
        
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _refreshAccessHandler.Handle(command, CancellationToken.None));
    }
}