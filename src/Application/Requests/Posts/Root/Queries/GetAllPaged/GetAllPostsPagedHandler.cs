using Application.Common.Pagination;
using Application.Contracts.Persistence.Repositories.Post;
using Application.Contracts.Services;
using Application.Responses;
using Cortex.Mediator.Queries;

namespace Application.Requests.Posts.Root.Queries.GetAllPaged;

public class GetAllPostsPagedHandler(
    IPostRepository postRepository,
    ICacheService cache
    ) : IQueryHandler<GetAllPostsPagedQuery, PagedResult<PostResponseDto>>
{
    public async Task<PagedResult<PostResponseDto>> Handle(GetAllPostsPagedQuery request, CancellationToken cancellationToken)
    {
        var page = request.PageNumber;
        var size = request.PageSize;
        
        var cachedList = await cache.GetAsync<PagedResult<PostResponseDto>>($"posts-{page}-{size}", cancellationToken);
        if (cachedList is not null) return cachedList;
        
        var listFromDb = await postRepository.GetDtoPagedAsync(page, size);
        await cache.SetAsync($"posts-{page}-{size}", listFromDb, TimeSpan.FromMinutes(2), cancellationToken);
        
        return listFromDb;
    }
}