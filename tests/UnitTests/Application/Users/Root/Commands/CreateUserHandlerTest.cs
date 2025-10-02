using System.Linq.Expressions;
using Application;
using Application.Contracts.Services;
using Application.Requests.Users.Root.Commands.CreateUser;
using Application.Responses;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;
using FluentValidation;
using Moq;
using UnitTests.Application.Users.Common;
using UnitTests.Extensions;

namespace UnitTests.Application.Users.Root.Commands;

public class CreateUserHandlerTest : BaseUserHandlerTest
{
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly IdentityUserCreationResult _result;
    private readonly CreateUserHandler _createUserHandler;

    public CreateUserHandlerTest()
    {
        _result = new IdentityUserCreationResult
        {
            Succeeded = true,
            Errors = []
        };
        
        _createUserHandler = new CreateUserHandler(
            UserRepositoryMock.Object,
            _tokenServiceMock.Object,
            AuthServiceMock.Object,
            Mapper
        );
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreateAndSaveAppUserAndAppIdentityUser()
    {
        // Arrange
        var command = new CreateUserCommand("validUser", "validemail@example.com", "Test-Password-123", "Test", "User", "1990-01-01");
        
        UserRepositoryMock.SetupUserExistsByAnyFilters(false);
        UserRepositoryMock.SetupSaveChanges();

        AuthServiceMock.SetupCreateIdentityUserFromAppUserAsync(_result);
        _tokenServiceMock.SetupCreateToken();

        // Act
        await _createUserHandler.Handle(command, CancellationToken.None);

        // Assert
        UserRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Once);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        AuthServiceMock.Verify(x => x.CreateIdentityUserFromAppUserAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _tokenServiceMock.Verify(x => x.CreateToken(It.IsAny<AppUser>(), It.IsAny<IEnumerable<string>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUsernameOrEmailAlreadyExists_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new CreateUserCommand("username", "validemail@example.com", "Test-Password-123", "Test", "User", "1990-01-01");
        
        UserRepositoryMock.SetupUserExistsByAnyFilters(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createUserHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Never);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAgeIsTooYoung_ShouldThrowBadRequestException()
    {
        // Arrange
        var youngDateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-10)).ToString("yyyy-MM-dd");
        var command = new CreateUserCommand("username", "test@email.com", "Test-Password-123", "Test", "User", youngDateOfBirth);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createUserHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Never);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenDateOfBirthIsInFuture_ShouldThrowBadRequestException()
    {
        // Arrange
        var tomorrowDateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddDays(1)).ToString("yyyy-MM-dd");
        var command = new CreateUserCommand("username", "test@email.com", "Test-Password-123", "Test", "User", tomorrowDateOfBirth);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createUserHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Never);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenDateOfBirthIsBefore1900_ShouldThrowBadRequestException()
    {
        // Arrange
        var dateOfBirth1899 = DateOnly.Parse("1899-01-01").ToString();
        var command = new CreateUserCommand("username", "test@email.com", "Test-Password-123", "Test", "User", dateOfBirth1899);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createUserHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Never);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenInvalidUsername_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new CreateUserCommand("a", "test@email.com", "Test-Password-123", "Test", "User", "1990-01-01");

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createUserHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Never);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenAuthUserCreationFails_ShouldThrowValidationException()
    {
        // Arrange
        var command = new CreateUserCommand("username", "test@email.com", "Test-Password-123", "Test", "User", "1990-01-01");
        var failedCreationResult = new IdentityUserCreationResult
        {
            Succeeded = false,
            Errors = ["Some error message."]
        };
        
        UserRepositoryMock.SetupUserExistsByAnyFilters(false);
        AuthServiceMock.SetupCreateIdentityUserFromAppUserAsync(failedCreationResult);
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _createUserHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        AuthServiceMock.Verify(x => x.CreateIdentityUserFromAppUserAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Never);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        AuthServiceMock.Verify(x => x.DeleteIdentityUserAsync(It.IsAny<AppUser>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCannotSaveChanges_ShouldDeleteSavedAuthUser_ThenThrowBadRequestException()
    {
        // Arrange
        var command = new CreateUserCommand("username", "test@email.com", "Test-Password-123", "Test", "User", "1990-01-01");

        UserRepositoryMock.SetupUserExistsByAnyFilters(false);
        AuthServiceMock.SetupCreateIdentityUserFromAppUserAsync(_result);
        UserRepositoryMock.SetupSaveChanges(false);
        
        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createUserHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        AuthServiceMock.Verify(x => x.CreateIdentityUserFromAppUserAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Once);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        AuthServiceMock.Verify(x => x.DeleteIdentityUserAsync(It.IsAny<AppUser>()), Times.Once);
    }
}