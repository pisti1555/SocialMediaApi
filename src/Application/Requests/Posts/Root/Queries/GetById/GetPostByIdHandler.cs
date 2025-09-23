using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories;
using Application.Contracts.Services;
using Application.Responses;
using Cortex.Mediator.Queries;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;

namespace Application.Requests.Posts.Root.Queries.GetById;

public class GetPostByIdHandler(
    IRepository<Post, PostResponseDto> repository, 
    ICacheService cache
) : IQueryHandler<GetPostByIdQuery, PostResponseDto>
{
    public async Task<PostResponseDto> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var guid = Parser.ParseIdOrThrow(request.Id);
        
        var cacheKey = $"post-{request.Id}";
        
        var cachedPost = await cache.GetAsync<PostResponseDto>(cacheKey, cancellationToken);
        if (cachedPost is not null) return cachedPost;
        
        var post = await repository.GetByIdAsync(guid, cancellationToken);
        if (post is null) throw new NotFoundException("Post not found.");
        
        await cache.SetAsync(cacheKey, post, TimeSpan.FromMinutes(10), cancellationToken);
        
        return post;
    }
}