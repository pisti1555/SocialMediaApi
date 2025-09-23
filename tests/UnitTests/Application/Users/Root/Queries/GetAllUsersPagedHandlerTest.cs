using Application.Common.Pagination;
using Application.Requests.Users.Root.Queries.GetAllPaged;
using Application.Responses;
using UnitTests.Application.Users.Common;
using UnitTests.Extensions;
using UnitTests.Factories;

namespace UnitTests.Application.Users.Root.Queries;

public class GetAllUsersPagedHandlerTest : BaseUserHandlerTest
{
    private readonly GetAllUsersPagedHandler _handler;

    private readonly PagedResult<UserResponseDto> _users;
    private readonly string _usersCacheKey;

    public GetAllUsersPagedHandlerTest()
    {
        _handler = new GetAllUsersPagedHandler(UserRepositoryMock.Object, CacheServiceMock.Object);
        
        var usersList = TestDataFactory.CreateUsers(5);
        var userResponseDtoList = Mapper.Map<List<UserResponseDto>>(usersList);
        _users = PagedResult<UserResponseDto>.Create(userResponseDtoList, userResponseDtoList.Count, 1, 10);
        _usersCacheKey = $"users-{Query.PageNumber}-{Query.PageSize}";
    }
    
    private static readonly GetAllUsersPagedQuery Query = new()
    {
        PageNumber = 1,
        PageSize = 10
    };
    
    private static void AssertUsersMatch(PagedResult<UserResponseDto> expected, PagedResult<UserResponseDto> actual)
    {
        Assert.NotNull(actual);
        Assert.NotNull(actual.Items);
        Assert.Equal(expected.TotalCount, actual.TotalCount);
        Assert.Equal(expected.Items.Count, actual.Items.Count);
        
        for (var i = 0; i < expected.Items.Count; i++)
        {
            Assert.Equal(expected.Items[i].Id, actual.Items[i].Id);
            Assert.Equal(expected.Items[i].UserName, actual.Items[i].UserName);
            Assert.Equal(expected.Items[i].Email, actual.Items[i].Email);
            Assert.Equal(expected.Items[i].FirstName, actual.Items[i].FirstName);
            Assert.Equal(expected.Items[i].LastName, actual.Items[i].LastName);
            Assert.Equal(expected.Items[i].DateOfBirth, actual.Items[i].DateOfBirth);
            Assert.Equal(expected.Items[i].CreatedAt, actual.Items[i].CreatedAt);
        }
    }

    [Fact]
    public async Task Handle_WhenNoCacheExists_ShouldReturnPagedResultFromDatabase()
    {
        // Arrange
        CacheServiceMock.SetupCache<PagedResult<UserResponseDto>?>(_usersCacheKey, null);
        UserRepositoryMock.SetupGetPaged(_users);

        // Act
        var result = await _handler.Handle(Query, CancellationToken.None);

        // Assert
        CacheServiceMock.VerifyCacheHit<PagedResult<UserResponseDto>?>(_usersCacheKey);
        UserRepositoryMock.VerifyGetPaged();
        
        AssertUsersMatch(_users, result);
    }
    
    [Fact]
    public async Task Handle_WhenCacheExists_ShouldReturnPagedResultFromCache()
    {
        // Arrange
        CacheServiceMock.SetupCache<PagedResult<UserResponseDto>?>(_usersCacheKey, _users);
        UserRepositoryMock.SetupGetPaged(_users);

        // Act
        var result = await _handler.Handle(Query, CancellationToken.None);

        // Assert
        CacheServiceMock.VerifyCacheHit<PagedResult<UserResponseDto>?>(_usersCacheKey);
        UserRepositoryMock.VerifyGetPaged(called: false);
        
        AssertUsersMatch(_users, result);
    }

    [Fact]
    public async Task Handle_WhenNoItemsFound_ShouldReturnEmptyResult()
    {
        // Arrange
        CacheServiceMock.SetupCache<PagedResult<UserResponseDto>?>(_usersCacheKey, null);
        UserRepositoryMock.SetupGetPaged(PagedResult<UserResponseDto>.Create([], 0, 1, 10));
        
        var result = await _handler.Handle(Query, CancellationToken.None);
        
        // Assert
        CacheServiceMock.VerifyCacheHit<PagedResult<UserResponseDto>?>(_usersCacheKey);
        UserRepositoryMock.VerifyGetPaged();
        
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }
}