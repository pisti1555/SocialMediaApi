using Application.Common.Pagination;
using Application.Contracts.Persistence.Repositories;
using Application.Responses;
using Cortex.Mediator.Queries;
using Domain.Users;

namespace Application.Requests.Users.Root.Queries.GetAllPaged;

public class GetAllUsersPagedHandler(
    IRepository<AppUser, UserResponseDto> userRepository
    ) : IQueryHandler<GetAllUsersPagedQuery, PagedResult<UserResponseDto>>
{
    public async Task<PagedResult<UserResponseDto>> Handle(GetAllUsersPagedQuery request, CancellationToken cancellationToken)
    {
        var page = request.PageNumber;
        var size = request.PageSize;
        
        return await userRepository.GetPagedAsync(page, size, null, cancellationToken);
    }
}