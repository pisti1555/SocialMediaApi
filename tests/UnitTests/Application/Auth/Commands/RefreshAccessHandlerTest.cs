using Application.Common.Adapters.Auth;
using Application.Common.Results;
using Application.Contracts.Services;
using Application.Requests.Auth.Commands.RefreshAccess;
using Domain.Common.Exceptions.CustomExceptions;
using Moq;
using UnitTests.Application.Users.Common;
using UnitTests.Extensions;
using UnitTests.Factories;

namespace UnitTests.Application.Auth.Commands;

public class RefreshAccessHandlerTest : BaseUserHandlerTest
{
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IHasher> _hasherMock = new();

    private readonly AccessTokenClaims _claims;
    
    private readonly RefreshAccessHandler _refreshAccessHandler;
    
    private const string OldAccessToken = "old-access-token";
    private const string OldRefreshToken = "old-refresh-token";
    private const string NewAccessToken = "new-access-token";
    private const string NewRefreshToken = "new-refresh-token";

    public RefreshAccessHandlerTest()
    {
        _claims = TestDataFactory.CreateAccessTokenClaims();
        _refreshAccessHandler = new RefreshAccessHandler(
            AuthServiceMock.Object, 
            _tokenServiceMock.Object,
            _hasherMock.Object
        );
    }
    
    [Fact]
    public async Task Handle_WhenValidRequest_ShouldReturnTokenResponseDto()
    {
        // Arrange
        _tokenServiceMock.SetupGetValidatedClaimsFromToken(AppResult<AccessTokenClaims?>.Success(_claims));
        _tokenServiceMock.SetupValidateToken(true);
        _tokenServiceMock.SetupCreateAccessToken(NewAccessToken);
        _tokenServiceMock.SetupCreateRefreshToken(NewRefreshToken);
        AuthServiceMock.SetupUpdateTokenAsync(AppResult.Success());
        _hasherMock.Setup(x => x.CreateHash(It.IsAny<string>())).Returns($"hashed-value");
        
        var command = new RefreshAccessCommand(OldAccessToken, OldRefreshToken);
        
        // Act
        var result = await _refreshAccessHandler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(NewAccessToken, result.AccessToken);
        Assert.Equal(NewRefreshToken, result.RefreshToken);
    }
    
    [Fact]
    public async Task Handle_WhenInvalidClaims_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _tokenServiceMock.SetupGetValidatedClaimsFromToken(AppResult<AccessTokenClaims?>.Failure(["Some error message"]));
        _tokenServiceMock.SetupValidateToken(true);
        _tokenServiceMock.SetupCreateAccessToken(NewAccessToken);
        _tokenServiceMock.SetupCreateRefreshToken(NewRefreshToken);
        AuthServiceMock.SetupUpdateTokenAsync(AppResult.Success());
        _hasherMock.Setup(x => x.CreateHash(It.IsAny<string>())).Returns("hashed-value");
        
        var command = new RefreshAccessCommand(OldAccessToken, OldRefreshToken);
        
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _refreshAccessHandler.Handle(command, CancellationToken.None));
    }
    
    [Fact]
    public async Task Handle_WhenInvalidToken_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _tokenServiceMock.SetupGetValidatedClaimsFromToken(AppResult<AccessTokenClaims?>.Success(_claims));
        _tokenServiceMock.SetupValidateToken(false);
        _tokenServiceMock.SetupCreateAccessToken(NewAccessToken);
        _tokenServiceMock.SetupCreateRefreshToken(NewRefreshToken);
        AuthServiceMock.SetupUpdateTokenAsync(AppResult.Success());
        _hasherMock.Setup(x => x.CreateHash(It.IsAny<string>())).Returns("hashed-value");
        
        var command = new RefreshAccessCommand(OldAccessToken, OldRefreshToken);
        
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _refreshAccessHandler.Handle(command, CancellationToken.None));
    }
    
    [Fact]
    public async Task Handle_WhenCannotUpdateToken_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _tokenServiceMock.SetupGetValidatedClaimsFromToken(AppResult<AccessTokenClaims?>.Success(_claims));
        _tokenServiceMock.SetupValidateToken(true);
        _tokenServiceMock.SetupCreateAccessToken(NewAccessToken);
        _tokenServiceMock.SetupCreateRefreshToken(NewRefreshToken);
        AuthServiceMock.SetupUpdateTokenAsync(AppResult.Failure(["Some error message."]));
        _hasherMock.Setup(x => x.CreateHash(It.IsAny<string>())).Returns("hashed-value");
        
        var command = new RefreshAccessCommand(OldAccessToken, OldRefreshToken);
        
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _refreshAccessHandler.Handle(command, CancellationToken.None));
    }
}