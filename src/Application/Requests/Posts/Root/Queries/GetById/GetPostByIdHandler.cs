using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories.Post;
using Application.Contracts.Services;
using Application.Responses;
using AutoMapper;
using Cortex.Mediator.Queries;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;

namespace Application.Requests.Posts.Root.Queries.GetById;

public class GetPostByIdHandler(
    IPostRepository postRepository, 
    ICacheService cache,
    IMapper mapper) : IQueryHandler<GetPostByIdQuery, PostResponseDto>
{
    public async Task<PostResponseDto> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var guid = Parser.ParseIdOrThrow(request.Id);
        
        var cacheKey = $"post-{request.Id}";
        
        var cachedPost = await cache.GetAsync<Post>(cacheKey, cancellationToken);
        if (cachedPost is not null) return mapper.Map<PostResponseDto>(cachedPost);
        
        var post = await postRepository.GetByIdAsync(guid);
        if (post is null) throw new NotFoundException("Post not found.");
        
        await cache.SetAsync(cacheKey, post, TimeSpan.FromMinutes(10), cancellationToken);
        
        return mapper.Map<PostResponseDto>(post);
    }
}