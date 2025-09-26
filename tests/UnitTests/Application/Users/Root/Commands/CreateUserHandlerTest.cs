using System.Linq.Expressions;
using Application.Requests.Users.Root.Commands.CreateUser;
using Application.Responses;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;
using Moq;
using UnitTests.Application.Users.Common;
using UnitTests.Extensions;

namespace UnitTests.Application.Users.Root.Commands;

public class CreateUserHandlerTest : BaseUserHandlerTest
{
    private readonly CreateUserHandler _createUserHandler;

    public CreateUserHandlerTest()
    {
        _createUserHandler = new CreateUserHandler(UserRepositoryMock.Object, CacheServiceMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreateAndSaveAndCacheUser()
    {
        // Arrange
        var command = new CreateUserCommand("validUser", "validemail@example.com", "Test", "User", "1990-01-01");
        
        UserRepositoryMock.SetupUserExistsByAnyFilters(false);
        UserRepositoryMock.SetupSaveChanges();

        // Act
        await _createUserHandler.Handle(command, CancellationToken.None);

        // Assert
        UserRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Once);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        CacheServiceMock.VerifyCacheSet<UserResponseDto?>();
    }

    [Fact]
    public async Task Handle_WhenUsernameOrEmailAlreadyExists_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new CreateUserCommand("username", "validemail@example.com", "Test", "User", "1990-01-01");
        
        UserRepositoryMock.SetupUserExistsByAnyFilters(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createUserHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<AppUser, bool>>>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Never);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        CacheServiceMock.VerifyCacheSet<UserResponseDto?>(happened: false);
    }

    [Fact]
    public async Task Handle_WhenAgeIsTooYoung_ShouldThrowBadRequestException()
    {
        // Arrange
        var youngDateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-10)).ToString("yyyy-MM-dd");
        var command = new CreateUserCommand("username", "test@email.com", "Test", "User", youngDateOfBirth);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createUserHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Never);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        CacheServiceMock.VerifyCacheSet<UserResponseDto?>(happened: false);
    }
    
    [Fact]
    public async Task Handle_WhenDateOfBirthIsInFuture_ShouldThrowBadRequestException()
    {
        // Arrange
        var tomorrowDateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddDays(1)).ToString("yyyy-MM-dd");
        var command = new CreateUserCommand("username", "test@email.com", "Test", "User", tomorrowDateOfBirth);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createUserHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Never);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        CacheServiceMock.VerifyCacheSet<UserResponseDto?>(happened: false);
    }
    
    [Fact]
    public async Task Handle_WhenDateOfBirthIsBefore1900_ShouldThrowBadRequestException()
    {
        // Arrange
        var dateOfBirth1899 = DateOnly.Parse("1899-01-01").ToString();
        var command = new CreateUserCommand("username", "test@email.com", "Test", "User", dateOfBirth1899);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createUserHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Never);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        CacheServiceMock.VerifyCacheSet<UserResponseDto?>(happened: false);
    }

    [Fact]
    public async Task Handle_WhenInvalidUsername_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new CreateUserCommand("a", "test@email.com", "Test", "User", "1990-01-01");

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createUserHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Never);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        CacheServiceMock.VerifyCacheSet<UserResponseDto?>(happened: false);
    }
}