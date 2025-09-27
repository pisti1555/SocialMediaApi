using Application.Common.Pagination;
using Application.Contracts.Persistence.Repositories;
using Application.Contracts.Services;
using Application.Responses;
using Cortex.Mediator.Queries;
using Domain.Posts;

namespace Application.Requests.Posts.Root.Queries.GetAllPaged;

public class GetAllPostsPagedHandler(
    IRepository<Post, PostResponseDto> repository, 
    ICacheService cache
    ) : IQueryHandler<GetAllPostsPagedQuery, PagedResult<PostResponseDto>>
{
    public async Task<PagedResult<PostResponseDto>> Handle(GetAllPostsPagedQuery request, CancellationToken cancellationToken)
    {
        var page = request.PageNumber;
        var size = request.PageSize;
        
        var cachedList = await cache.GetAsync<PagedResult<PostResponseDto>>($"posts-{page}-{size}", cancellationToken);
        if (cachedList is not null) return cachedList;
        
        var listFromDb = await repository.GetPagedAsync(page, size, null, cancellationToken);
        await cache.SetAsync($"posts-{page}-{size}", listFromDb, TimeSpan.FromMinutes(2), cancellationToken);
        
        return listFromDb;
    }
}