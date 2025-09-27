using Application.Common.Pagination;
using Application.Contracts.Persistence.Repositories;
using Application.Contracts.Services;
using Application.Responses;
using Cortex.Mediator.Queries;
using Domain.Users;

namespace Application.Requests.Users.Root.Queries.GetAllPaged;

public class GetAllUsersPagedHandler(
    IRepository<AppUser, UserResponseDto> userRepository,
    ICacheService cache
    ) : IQueryHandler<GetAllUsersPagedQuery, PagedResult<UserResponseDto>>
{
    public async Task<PagedResult<UserResponseDto>> Handle(GetAllUsersPagedQuery request, CancellationToken cancellationToken)
    {
        var page = request.PageNumber;
        var size = request.PageSize;
        
        var cachedList = await cache.GetAsync<PagedResult<UserResponseDto>>($"users-{page}-{size}", cancellationToken);
        if (cachedList is not null) return cachedList;
        
        var listFromDb = await userRepository.GetPagedAsync(page, size, null, cancellationToken);
        await cache.SetAsync($"users-{page}-{size}", listFromDb, TimeSpan.FromMinutes(10), cancellationToken);
        
        return listFromDb;
    }
}