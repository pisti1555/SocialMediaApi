using Application.Common.Pagination;
using Application.Contracts.Persistence.Repositories.Post;
using Application.Responses;
using Cortex.Mediator.Queries;

namespace Application.Requests.Posts.Root.Queries.GetAllPaged;

public class GetAllPostsPagedHandler(IPostRepository postRepository) : IQueryHandler<GetAllPostsPagedQuery, PagedResult<PostResponseDto>>
{
    public async Task<PagedResult<PostResponseDto>> Handle(GetAllPostsPagedQuery request, CancellationToken cancellationToken)
    {
        return await postRepository.GetDtoPagedAsync(request.PageNumber, request.PageSize);
    }
}