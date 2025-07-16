using Application.Common.Interfaces.Repositories;
using Application.Common.Pagination;
using Application.Responses;
using MediatR;

namespace Application.Requests.Posts.Root.Queries.GetAllPaged;

public class GetAllPostsPagedHandler(IPostRepository postRepository) : IRequestHandler<GetAllPostsPagedQuery, PagedResult<PostResponseDto>>
{
    public async Task<PagedResult<PostResponseDto>> Handle(GetAllPostsPagedQuery request, CancellationToken cancellationToken)
    {
        return await postRepository.GetPostDtoPagedAsync(request);
    }
}