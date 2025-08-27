using Application.Common.Interfaces.Persistence.Repositories.AppUser;
using Application.Common.Mappings;
using Application.Requests.Users.Root.Queries.GetById;
using ApplicationUnitTests.Common;
using AutoMapper;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;
using Moq;

namespace ApplicationUnitTests.Requests.Users.Root.Queries.GetById;

public class GetUserByIdHandlerTest
{
    private readonly Mock<IAppUserRepository> _userRepositoryMock;
    private readonly GetUserByIdHandler _handler;

    public GetUserByIdHandlerTest()
    {
        _userRepositoryMock = new Mock<IAppUserRepository>();
        
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<UserProfile>());
        mapperConfig.AssertConfigurationIsValid();

        var mapper = mapperConfig.CreateMapper();
        
        _handler = new GetUserByIdHandler(_userRepositoryMock.Object, mapper);
    }

    [Fact]
    public async Task Handle_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId.ToString());

        var user = TestObjects.CreateTestUser();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(result.Id, user.Id);
        Assert.Equal(result.Email, user.Email);
        Assert.Equal(result.FirstName, user.FirstName);
        Assert.Equal(result.LastName, user.LastName);
        Assert.Equal(result.DateOfBirth, user.DateOfBirth);
        Assert.Equal(result.UserName, user.UserName);
        Assert.Equal(result.CreatedAt, user.CreatedAt);
        Assert.Equal(result.LastActive, user.LastActive);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_UserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId.ToString());

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((AppUser?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_InvalidGUID()
    {
        // Arrange
        var query = new GetUserByIdQuery("invalid-guid");

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(query, CancellationToken.None));
    }
}