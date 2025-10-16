using System.Linq.Expressions;
using Application.Common.Adapters.Auth;
using Application.Common.Results;
using Application.Contracts.Services;
using Application.Requests.Auth.Commands.Registration;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;
using FluentValidation;
using Moq;
using UnitTests.Application.Users.Common;
using UnitTests.Extensions;
using UnitTests.Factories;

namespace UnitTests.Application.Auth.Commands;

public class RegistrationHandlerTest : BaseUserHandlerTest
{
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IHasher> _hasherMock = new();
    private readonly AppResult _result;
    private readonly RegistrationHandler _registrationHandler;
    
    private readonly AccessTokenClaims _claims;

    public RegistrationHandlerTest()
    {
        _result = AppResult.Success();
        
        _registrationHandler = new RegistrationHandler(
            UserRepositoryMock.Object,
            _tokenServiceMock.Object,
            AuthServiceMock.Object,
            _hasherMock.Object,
            Mapper
        );
        
        _claims = TestDataFactory.CreateAccessTokenClaims();
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreateAndSaveAppUserAndAppIdentityUser()
    {
        // Arrange
        var command = new RegistrationCommand(
            "validUser", 
            "validemail@example.com", 
            "Test-Password-123", 
            "Test", 
            "User", 
            "1990-01-01",
            false
        );
        
        UserRepositoryMock.SetupUserExistsByAnyFilters(false);
        UserRepositoryMock.SetupSaveChanges();

        AuthServiceMock.SetupCreateIdentityUserAsync(_result);
        AuthServiceMock.SetupSaveTokenAsync(AppResult.Success());
        
        _tokenServiceMock.SetupGetValidatedClaimsFromToken(AppResult<AccessTokenClaims?>.Success(_claims));
        _tokenServiceMock.SetupCreateAccessToken();
        _tokenServiceMock.SetupCreateRefreshToken();

        // Act
        await _registrationHandler.Handle(command, CancellationToken.None);

        // Assert
        UserRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Once);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        AuthServiceMock.Verify(x => x.CreateIdentityUserAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _tokenServiceMock.Verify(x => x.CreateAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>()), Times.Once);
        AuthServiceMock.Verify(x => x.SaveTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        AuthServiceMock.Verify(x => x.DeleteIdentityUserAsync(It.IsAny<AppUser>()), Times.Never);
        UserRepositoryMock.Verify(x => x.Delete(It.IsAny<AppUser>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUsernameOrEmailAlreadyExists_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new RegistrationCommand(
            "username", 
            "validemail@example.com", 
            "Test-Password-123", 
            "Test", 
            "User", 
            "1990-01-01",
            false
        );
        
        UserRepositoryMock.SetupUserExistsByAnyFilters(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _registrationHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Never);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenInvalidUsername_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new RegistrationCommand(
            "a", 
            "test@email.com", 
            "Test-Password-123", 
            "Test", 
            "User", 
            "1990-01-01",
            false
        );

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _registrationHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Never);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenAuthUserCreationFails_ShouldThrowValidationException()
    {
        // Arrange
        var command = new RegistrationCommand(
            "username", 
            "test@email.com", 
            "Test-Password-123", 
            "Test", 
            "User", 
            "1990-01-01",
            false
        );
        var failedCreationResult = AppResult.Failure(["Some error message."]);
        
        UserRepositoryMock.SetupUserExistsByAnyFilters(false);
        AuthServiceMock.SetupCreateIdentityUserAsync(failedCreationResult);
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _registrationHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        AuthServiceMock.Verify(x => x.CreateIdentityUserAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Never);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        AuthServiceMock.Verify(x => x.DeleteIdentityUserAsync(It.IsAny<AppUser>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCannotSaveChanges_ShouldDeleteSavedAuthUser_ThenThrowBadRequestException()
    {
        // Arrange
        var command = new RegistrationCommand(
            "username", 
            "test@email.com", 
            "Test-Password-123", 
            "Test", 
            "User", 
            "1990-01-01",
            false
        );

        UserRepositoryMock.SetupUserExistsByAnyFilters(false);
        AuthServiceMock.SetupCreateIdentityUserAsync(_result);
        UserRepositoryMock.SetupSaveChanges(false);
        
        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _registrationHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        AuthServiceMock.Verify(x => x.CreateIdentityUserAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Once);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        AuthServiceMock.Verify(x => x.DeleteIdentityUserAsync(It.IsAny<AppUser>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WhenCannotSaveToken_ShouldDeleteSavedAuthUserAndAppUser_ThenThrowBadRequestException()
    {
        // Arrange
        var command = new RegistrationCommand(
            "validUser", 
            "validemail@example.com", 
            "Test-Password-123", 
            "Test", 
            "User", 
            "1990-01-01",
            false
        );
        
        UserRepositoryMock.SetupUserExistsByAnyFilters(false);
        UserRepositoryMock.SetupSaveChanges();

        AuthServiceMock.SetupCreateIdentityUserAsync(_result);
        AuthServiceMock.SetupSaveTokenAsync(AppResult.Failure(["Some error message."]));
        
        _tokenServiceMock.SetupGetValidatedClaimsFromToken(AppResult<AccessTokenClaims?>.Success(_claims));
        _tokenServiceMock.SetupCreateAccessToken();
        _tokenServiceMock.SetupCreateRefreshToken();

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _registrationHandler.Handle(command, CancellationToken.None));

        UserRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Once);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        AuthServiceMock.Verify(x => x.CreateIdentityUserAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _tokenServiceMock.Verify(x => x.CreateAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>()), Times.Once);
        AuthServiceMock.Verify(x => x.SaveTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        AuthServiceMock.Verify(x => x.DeleteIdentityUserAsync(It.IsAny<AppUser>()), Times.Once);
        UserRepositoryMock.Verify(x => x.Delete(It.IsAny<AppUser>()), Times.Once);
    }
}