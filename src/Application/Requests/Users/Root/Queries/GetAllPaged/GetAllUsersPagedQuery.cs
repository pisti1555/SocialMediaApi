using Application.Common.Pagination;
using Application.Responses;
using MediatR;

namespace Application.Requests.Users.Root.Queries.GetAllPaged;

public record GetAllUsersPagedQuery : PaginationAttributes, IRequest<PagedResult<UserResponseDto>>;