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
        var command = new CreateUserCommand("validUser", "validemail@example.com", "Test", "User",
            DateOnly.Parse("1990-01-01"));
        
        UserRepositoryMock.Setup(x => x.ExistsByUsernameAsync(command.UserName)).ReturnsAsync(false);
        UserRepositoryMock.Setup(x => x.ExistsByEmailAsync(command.Email)).ReturnsAsync(false);
        UserRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        await _createUserHandler.Handle(command, CancellationToken.None);

        // Assert
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Once);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        CacheServiceMock.VerifyCacheSet<UserResponseDto?>();
    }

    [Fact]
    public async Task Handle_WhenUsernameAlreadyExists_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new CreateUserCommand("username", "validemail@example.com", "Test", "User",
            DateOnly.Parse("1990-01-01"));
        
        UserRepositoryMock.Setup(x => x.ExistsByUsernameAsync(command.UserName)).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createUserHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Never);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        CacheServiceMock.VerifyCacheSet<UserResponseDto?>(happened: false);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new CreateUserCommand("username", "test@email.com", "Test", "User",
            DateOnly.Parse("1990-01-01"));
        
        UserRepositoryMock.Setup(x => x.ExistsByUsernameAsync(command.UserName)).ReturnsAsync(false);
        UserRepositoryMock.Setup(x => x.ExistsByEmailAsync(command.Email)).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createUserHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Never);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        CacheServiceMock.VerifyCacheSet<UserResponseDto?>(happened: false);
    }

    [Fact]
    public async Task Handle_WhenAgeIsTooYoung_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new CreateUserCommand("username", "test@email.com", "Test", "User",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-10)));

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createUserHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Never);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        CacheServiceMock.VerifyCacheSet<UserResponseDto?>(happened: false);
    }

    [Fact]
    public async Task Handle_WhenInvalidUsername_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new CreateUserCommand("a", "test@email.com", "Test", "User", DateOnly.Parse("1990-01-01"));

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createUserHandler.Handle(command, CancellationToken.None));
        
        UserRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Never);
        UserRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        CacheServiceMock.VerifyCacheSet<UserResponseDto?>(happened: false);
    }
}