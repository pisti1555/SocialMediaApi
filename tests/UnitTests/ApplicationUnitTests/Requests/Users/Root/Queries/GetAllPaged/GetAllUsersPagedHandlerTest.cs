using Application.Common.Pagination;
using Application.Contracts.Persistence.Repositories.AppUser;
using Application.Requests.Users.Root.Queries.GetAllPaged;
using Application.Responses;
using Moq;

namespace ApplicationUnitTests.Requests.Users.Root.Queries.GetAllPaged;

public class GetAllUsersPagedHandlerTest
{
    private readonly Mock<IAppUserRepository> _userRepositoryMock;
    private readonly GetAllUsersPagedHandler _handler;

    public GetAllUsersPagedHandlerTest()
    {
        _userRepositoryMock = new Mock<IAppUserRepository>();
        _handler = new GetAllUsersPagedHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSucceed()
    {
        // Arrange
        var query = new GetAllUsersPagedQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        var items = new List<UserResponseDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserName = "user1",
                Email = "user1@email.com",
                FirstName = "User",
                LastName = "One",
                DateOfBirth = new DateOnly(1990, 1, 1),
                CreatedAt = DateTime.UtcNow,
                LastActive = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserName = "user2",
                Email = "user2@email.com",
                FirstName = "User",
                LastName = "Two",
                DateOfBirth = new DateOnly(1991, 2, 2),
                CreatedAt = DateTime.UtcNow,
                LastActive = DateTime.UtcNow
            }
        };

        var pagedResult = PagedResult<UserResponseDto>.Create(items, 2, 1, 10);

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

        _userRepositoryMock.Verify(x => x.GetAllDtoPagedAsync(query.PageNumber, query.PageSize), Times.Once);
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

        _userRepositoryMock.Verify(x => x.GetAllDtoPagedAsync(query.PageNumber, query.PageSize), Times.Once);
    }
}