using Application.Common.Interfaces.Repositories;
using Application.Common.Pagination;
using Application.Responses;
using MediatR;

namespace Application.Requests.Users.Root.Queries.GetAllPaged;

public class GetAllUsersPagedHandler(IAppUserRepository userRepository) : IRequestHandler<GetAllUsersPagedQuery, PagedResult<UserResponseDto>>
{
    public async Task<PagedResult<UserResponseDto>> Handle(GetAllUsersPagedQuery request, CancellationToken cancellationToken)
    {
        return await userRepository.GetAllDtoPagedAsync(request);
    }
}