using Application.Common.Pagination;
using Application.Contracts.Persistence.Repositories.AppUser;
using Application.Responses;
using Cortex.Mediator.Queries;

namespace Application.Requests.Users.Root.Queries.GetAllPaged;

public class GetAllUsersPagedHandler(IAppUserRepository userRepository) : IQueryHandler<GetAllUsersPagedQuery, PagedResult<UserResponseDto>>
{
    public async Task<PagedResult<UserResponseDto>> Handle(GetAllUsersPagedQuery request, CancellationToken cancellationToken)
    {
        return await userRepository.GetAllDtoPagedAsync(request.PageNumber, request.PageSize);
    }
}