using Application.Common.Pagination;
using Application.Responses;
using Cortex.Mediator.Queries;

namespace Application.Requests.Posts.Root.Queries.GetAllPaged;

public record GetAllPostsPagedQuery : PaginationAttributes, IQuery<PagedResult<PostResponseDto>>;