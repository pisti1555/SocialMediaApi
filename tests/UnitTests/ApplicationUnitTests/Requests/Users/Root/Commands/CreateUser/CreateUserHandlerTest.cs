using Application.Common.Interfaces.Persistence.Repositories.AppUser;
using Application.Requests.Users.Root.Commands.CreateUser;
using ApplicationUnitTests.Common;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;
using Moq;

namespace ApplicationUnitTests.Requests.Users.Root.Commands.CreateUser;

public class CreateUserHandlerTest
{
    private readonly Mock<IAppUserRepository> _userRepositoryMock;
    private readonly CreateUserHandler _createUserHandler;

    public CreateUserHandlerTest()
    {
        _userRepositoryMock = new Mock<IAppUserRepository>();
        _createUserHandler = new CreateUserHandler(_userRepositoryMock.Object, TestMapperSetup.SetupMapper());
    }

    [Fact]
    public async Task Handle_ShouldSucceed()
    {
        // Arrange
        var command = new CreateUserCommand("validUser", "validemail@example.com", "Test", "User",
            DateOnly.Parse("1990-01-01"));
        
        _userRepositoryMock.Setup(x => x.ExistsByUsernameAsync(command.UserName)).ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(command.Email)).ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        await _createUserHandler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_UsernameAlreadyExists()
    {
        // Arrange
        var command = new CreateUserCommand("username", "validemail@example.com", "Test", "User",
            DateOnly.Parse("1990-01-01"));
        _userRepositoryMock.Setup(x => x.ExistsByUsernameAsync(command.UserName)).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createUserHandler.Handle(command, CancellationToken.None));
        _userRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Never);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_EmailAlreadyExists()
    {
        // Arrange
        var command = new CreateUserCommand("username", "test@email.com", "Test", "User",
            DateOnly.Parse("1990-01-01"));
        _userRepositoryMock.Setup(x => x.ExistsByUsernameAsync(command.UserName)).ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(command.Email)).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createUserHandler.Handle(command, CancellationToken.None));
        _userRepositoryMock.Verify(x => x.Add(It.IsAny<AppUser>()), Times.Never);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_AgeIsTooYoung()
    {
        // Arrange
        var command = new CreateUserCommand("username", "test@email.com", "Test", "User",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-10)));

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createUserHandler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_InvalidUsername()
    {
        // Arrange
        var command = new CreateUserCommand("a", "test@email.com", "Test", "User", DateOnly.Parse("1990-01-01"));

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _createUserHandler.Handle(command, CancellationToken.None));
    }
}