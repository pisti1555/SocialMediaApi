using Application.Common.Pagination;
using Application.Responses;
using MediatR;

namespace Application.Requests.Posts.Root.Queries.GetAllPaged;

public record GetAllPostsPagedQuery : PaginationAttributes, IRequest<PagedResult<PostResponseDto>>;