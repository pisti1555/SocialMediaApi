using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories.Post;
using Application.Contracts.Services;
using Application.Responses;
using AutoMapper;
using Cortex.Mediator.Queries;
using Domain.Common.Exceptions.CustomExceptions;

namespace Application.Requests.Posts.PostLike.Queries.GetLikesOfPost;

public class GetLikesOfPostHandler(
    IPostRepository postRepository,
    ICacheService cache,
    IMapper mapper
) : IQueryHandler<GetLikesOfPostQuery, List<PostLikeResponseDto>>
{
    public async Task<List<PostLikeResponseDto>> Handle(GetLikesOfPostQuery request, CancellationToken cancellationToken)
    {
        var guid = Parser.ParseIdOrThrow(request.PostId);
        if (!await postRepository.ExistsAsync(guid)) throw new NotFoundException("Post not found.");
        
        var cacheKey = $"post-likes-{request.PostId}";
        var cachedList = await cache.GetAsync<List<PostLikeResponseDto>>(cacheKey, cancellationToken);
        if (cachedList is not null) return cachedList;
        
        var result = await postRepository.LikeRepository.GetAllOfPostAsync(guid);
        var likeResponseList = mapper.Map<List<PostLikeResponseDto>>(result);
        
        await cache.SetAsync(cacheKey, likeResponseList, TimeSpan.FromMinutes(10), cancellationToken);
        
        return likeResponseList;
    }
}