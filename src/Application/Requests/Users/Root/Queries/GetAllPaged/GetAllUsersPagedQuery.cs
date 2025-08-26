using Application.Common.Pagination;
using Application.Responses;
using Cortex.Mediator.Queries;

namespace Application.Requests.Users.Root.Queries.GetAllPaged;

public record GetAllUsersPagedQuery : PaginationAttributes, IQuery<PagedResult<UserResponseDto>>;