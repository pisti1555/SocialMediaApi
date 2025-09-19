using Application.Requests.Users.Root.Queries.GetById;
using Application.Responses;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;
using Moq;
using UnitTests.Application.Users.Common;
using UnitTests.Extensions;
using UnitTests.Factories;

namespace UnitTests.Application.Users.Root.Queries;

public class GetUserByIdHandlerTest : BaseUserHandlerTest
{
    private readonly GetUserByIdHandler _handler;

    private readonly AppUser _user;
    private readonly string _userCacheKey;
    private readonly GetUserByIdQuery _query;

    public GetUserByIdHandlerTest()
    {
        _handler = new GetUserByIdHandler(UserRepositoryMock.Object, CacheServiceMock.Object, Mapper);
        
        _user = TestDataFactory.CreateUser();
        _userCacheKey = $"user-{_user.Id.ToString()}";
        _query = new GetUserByIdQuery(_user.Id.ToString());
    }

    private static void AssertUserMatch(AppUser expected, UserResponseDto actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Email, actual.Email);
        Assert.Equal(expected.FirstName, actual.FirstName);
        Assert.Equal(expected.LastName, actual.LastName);
        Assert.Equal(expected.DateOfBirth, actual.DateOfBirth);
        Assert.Equal(expected.UserName, actual.UserName);
        Assert.Equal(expected.CreatedAt, actual.CreatedAt);
        Assert.Equal(expected.LastActive, actual.LastActive);
    }

    [Fact]
    public async Task Handle_WhenCacheDoesNotExist_ShouldReturnUserFromDatabase()
    {
        // Arrange
        CacheServiceMock.SetupCache<UserResponseDto?>(_userCacheKey, null);
        UserRepositoryMock.SetupUser(_user);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        CacheServiceMock.VerifyCacheHit<UserResponseDto?>(_userCacheKey);
        UserRepositoryMock.Verify(x => x.GetByIdAsync(_user.Id), Times.Once);
        
        AssertUserMatch(_user, result);
    }
    
    [Fact]
    public async Task Handle_WhenCacheExists_ShouldReturnUserFromCache()
    {
        // Arrange
        CacheServiceMock.SetupCache<UserResponseDto?>(_userCacheKey, Mapper.Map<UserResponseDto>(_user));
        UserRepositoryMock.SetupUser(_user);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        CacheServiceMock.VerifyCacheHit<UserResponseDto?>(_userCacheKey);
        UserRepositoryMock.Verify(x => x.GetByIdAsync(_user.Id), Times.Never);
        
        AssertUserMatch(_user, result);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        CacheServiceMock.SetupCache<UserResponseDto?>(_userCacheKey, null);
        UserRepositoryMock.SetupUser(null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(_query, CancellationToken.None));
        
        CacheServiceMock.VerifyCacheHit<UserResponseDto?>(_userCacheKey);
        UserRepositoryMock.Verify(x => x.GetByIdAsync(_user.Id), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenInvalidGUID_ShouldThrowBadRequestException()
    {
        // Arrange
        var query = new GetUserByIdQuery("invalid-guid");

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(query, CancellationToken.None));
        
        CacheServiceMock.VerifyCacheHit<UserResponseDto?>(It.IsAny<string>(), false);
        UserRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }
}