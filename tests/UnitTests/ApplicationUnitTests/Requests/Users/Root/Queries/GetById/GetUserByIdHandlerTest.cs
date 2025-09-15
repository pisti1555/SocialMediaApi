using Application.Contracts.Persistence.Repositories.AppUser;
using Application.Contracts.Services;
using Application.Requests.Users.Root.Queries.GetById;
using ApplicationUnitTests.Factories;
using ApplicationUnitTests.Helpers;
using AutoMapper;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;
using Moq;

namespace ApplicationUnitTests.Requests.Users.Root.Queries.GetById;

public class GetUserByIdHandlerTest
{
    private readonly Mock<IAppUserRepository> _userRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly IMapper _mapper = MapperHelper.GetMapper();
    private readonly GetUserByIdHandler _handler;

    private readonly AppUser _user;
    private readonly string _userCacheKey;

    public GetUserByIdHandlerTest()
    {
        _userRepositoryMock = new Mock<IAppUserRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _handler = new GetUserByIdHandler(_userRepositoryMock.Object, _cacheServiceMock.Object, _mapper);
        
        _user = TestDataFactory.CreateUser();
        _userCacheKey = $"user-{_user.Id.ToString()}";
    }

    [Fact]
    public async Task Handle_ShouldReturnObject_FromDatabase()
    {
        // Arrange
        var query = new GetUserByIdQuery(_user.Id.ToString());

        _cacheServiceMock
            .Setup(x => x.GetAsync<AppUser>(_userCacheKey, CancellationToken.None))
            .ReturnsAsync((AppUser?)null);
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_user.Id))
            .ReturnsAsync(_user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _cacheServiceMock.Verify(x => x.GetAsync<AppUser>(_userCacheKey, CancellationToken.None), Times.Once);
        _userRepositoryMock.Verify(x => x.GetByIdAsync(_user.Id), Times.Once);
        
        Assert.NotNull(result);
        Assert.Equal(result.Id, _user.Id);
        Assert.Equal(result.Email, _user.Email);
        Assert.Equal(result.FirstName, _user.FirstName);
        Assert.Equal(result.LastName, _user.LastName);
        Assert.Equal(result.DateOfBirth, _user.DateOfBirth);
        Assert.Equal(result.UserName, _user.UserName);
        Assert.Equal(result.CreatedAt, _user.CreatedAt);
        Assert.Equal(result.LastActive, _user.LastActive);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnObject_FromCache()
    {
        // Arrange
        var query = new GetUserByIdQuery(_user.Id.ToString());

        _cacheServiceMock
            .Setup(x => x.GetAsync<AppUser>(_userCacheKey, CancellationToken.None))
            .ReturnsAsync(_user);
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_user.Id))
            .ReturnsAsync(_user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _cacheServiceMock.Verify(x => x.GetAsync<AppUser>(_userCacheKey, CancellationToken.None), Times.Once);
        _userRepositoryMock.Verify(x => x.GetByIdAsync(_user.Id), Times.Never);
        
        Assert.NotNull(result);
        Assert.Equal(result.Id, _user.Id);
        Assert.Equal(result.Email, _user.Email);
        Assert.Equal(result.FirstName, _user.FirstName);
        Assert.Equal(result.LastName, _user.LastName);
        Assert.Equal(result.DateOfBirth, _user.DateOfBirth);
        Assert.Equal(result.UserName, _user.UserName);
        Assert.Equal(result.CreatedAt, _user.CreatedAt);
        Assert.Equal(result.LastActive, _user.LastActive);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_UserDoesNotExist()
    {
        // Arrange
        var query = new GetUserByIdQuery(_user.Id.ToString());

        _cacheServiceMock
            .Setup(x => x.GetAsync<AppUser>(_userCacheKey, CancellationToken.None))
            .ReturnsAsync((AppUser?)null);
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_user.Id))
            .ReturnsAsync((AppUser?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
        _cacheServiceMock.Verify(x => x.GetAsync<AppUser>(_userCacheKey, CancellationToken.None), Times.Once);
        _userRepositoryMock.Verify(x => x.GetByIdAsync(_user.Id), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_InvalidGUID()
    {
        // Arrange
        var query = new GetUserByIdQuery("invalid-guid");

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(query, CancellationToken.None));
        _cacheServiceMock.Verify(x => x.GetAsync<AppUser>($"user-{query.Id}", CancellationToken.None), Times.Never);
        _userRepositoryMock.Verify(x => x.GetByIdAsync(Guid.Parse(query.Id)), Times.Never);
    }
}