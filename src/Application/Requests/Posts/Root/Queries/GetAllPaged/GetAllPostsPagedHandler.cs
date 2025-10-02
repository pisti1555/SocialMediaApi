using Application.Common.Pagination;
using Application.Contracts.Persistence.Repositories;
using Application.Responses;
using Cortex.Mediator.Queries;
using Domain.Posts;

namespace Application.Requests.Posts.Root.Queries.GetAllPaged;

public class GetAllPostsPagedHandler(
    IRepository<Post, PostResponseDto> repository
    ) : IQueryHandler<GetAllPostsPagedQuery, PagedResult<PostResponseDto>>
{
    public async Task<PagedResult<PostResponseDto>> Handle(GetAllPostsPagedQuery request, CancellationToken cancellationToken)
    {
        var page = request.PageNumber;
        var size = request.PageSize;
        
        return await repository.GetPagedAsync(page, size, null, cancellationToken);
    }
}