using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories;
using Application.Contracts.Services;
using Application.Responses;
using Cortex.Mediator.Queries;
using Domain.Common.Exceptions.CustomExceptions;
using XPost = Domain.Posts.Post;
using XLike = Domain.Posts.PostLike;

namespace Application.Requests.Posts.PostLike.Queries.GetLikesOfPost;

public class GetLikesOfPostHandler(
    IRepository<XPost, PostResponseDto> postRepository,
    IRepository<XLike, PostLikeResponseDto> likeRepository,
    ICacheService cache
) : IQueryHandler<GetLikesOfPostQuery, List<PostLikeResponseDto>>
{
    public async Task<List<PostLikeResponseDto>> Handle(GetLikesOfPostQuery request, CancellationToken cancellationToken)
    {
        var guid = Parser.ParseIdOrThrow(request.PostId);
        if (!await postRepository.ExistsAsync(guid, cancellationToken)) throw new NotFoundException("Post not found.");
        
        var cacheKey = $"post-likes-{request.PostId}";
        var cachedList = await cache.GetAsync<List<PostLikeResponseDto>>(cacheKey, cancellationToken);
        if (cachedList is not null) return cachedList;
        
        var result = await likeRepository.GetAllAsync(x => x.PostId == guid, cancellationToken);
        
        await cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10), cancellationToken);
        
        return result;
    }
}