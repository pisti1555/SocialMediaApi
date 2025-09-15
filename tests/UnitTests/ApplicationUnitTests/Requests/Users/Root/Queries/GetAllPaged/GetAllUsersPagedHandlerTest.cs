using Application.Common.Pagination;
using Application.Contracts.Persistence.Repositories.AppUser;
using Application.Contracts.Services;
using Application.Requests.Users.Root.Queries.GetAllPaged;
using Application.Responses;
using ApplicationUnitTests.Factories;
using ApplicationUnitTests.Helpers;
using Moq;

namespace ApplicationUnitTests.Requests.Users.Root.Queries.GetAllPaged;

public class GetAllUsersPagedHandlerTest
{
    private readonly Mock<IAppUserRepository> _userRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetAllUsersPagedHandler _handler;

    private readonly List<UserResponseDto> _users;

    public GetAllUsersPagedHandlerTest()
    {
        _userRepositoryMock = new Mock<IAppUserRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _handler = new GetAllUsersPagedHandler(_userRepositoryMock.Object, _cacheServiceMock.Object);
        
        _users = MapperHelper.GetMapper().Map<List<UserResponseDto>>(TestDataFactory.CreateUsers(5));
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResult_FromDatabase()
    {
        // Arrange
        var query = new GetAllUsersPagedQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        var pagedResult = PagedResult<UserResponseDto>.Create(_users, 5, 1, 10);

        _cacheServiceMock
            .Setup(x => x.GetAsync<PagedResult<UserResponseDto>>(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync((PagedResult<UserResponseDto>?)null);
        _userRepositoryMock
            .Setup(x => x.GetAllDtoPagedAsync(query.PageNumber, query.PageSize))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Equal(pagedResult.TotalCount, result.TotalCount);
        Assert.Equal(pagedResult.Items.Count, result.Items.Count);
        Assert.Equal(pagedResult.Items[0].UserName, pagedResult.Items[0].UserName);

        _cacheServiceMock.Verify(x => x.GetAsync<PagedResult<UserResponseDto>>(It.IsAny<string>(), CancellationToken.None), Times.Once);
        _userRepositoryMock.Verify(x => x.GetAllDtoPagedAsync(query.PageNumber, query.PageSize), Times.Once);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnPagedResult_FromCache()
    {
        // Arrange
        var query = new GetAllUsersPagedQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        var pagedResult = PagedResult<UserResponseDto>.Create(_users, 5, 1, 10);

        _cacheServiceMock
            .Setup(x => x.GetAsync<PagedResult<UserResponseDto>>(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(pagedResult);
        _userRepositoryMock
            .Setup(x => x.GetAllDtoPagedAsync(query.PageNumber, query.PageSize))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Equal(pagedResult.TotalCount, result.TotalCount);
        Assert.Equal(pagedResult.Items.Count, result.Items.Count);
        Assert.Equal(pagedResult.Items[0].UserName, pagedResult.Items[0].UserName);

        _cacheServiceMock.Verify(x => x.GetAsync<PagedResult<UserResponseDto>>(It.IsAny<string>(), CancellationToken.None), Times.Once);
        _userRepositoryMock.Verify(x => x.GetAllDtoPagedAsync(query.PageNumber, query.PageSize), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WithEmptyResult()
    {
        // Arrange
        var query = new GetAllUsersPagedQuery
        {
            PageNumber = 1,
            PageSize = 10
        };
        
        var pagedResult = PagedResult<UserResponseDto>.Create(new List<UserResponseDto>(), 0, 1, 10);

        _userRepositoryMock
            .Setup(x => x.GetAllDtoPagedAsync(query.PageNumber, query.PageSize))
            .ReturnsAsync(pagedResult);
        
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Empty(result.Items);
        Assert.Equal(pagedResult.TotalCount, result.TotalCount);
        Assert.Equal(pagedResult.Items.Count, result.Items.Count);
        Assert.Empty(pagedResult.Items);

        _cacheServiceMock.Verify(x => x.GetAsync<PagedResult<UserResponseDto>>(It.IsAny<string>(), CancellationToken.None), Times.Once);
        _userRepositoryMock.Verify(x => x.GetAllDtoPagedAsync(query.PageNumber, query.PageSize), Times.Once);
    }
}