using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories.Post;
using Application.Contracts.Services;
using Application.Responses;
using AutoMapper;
using Cortex.Mediator.Queries;
using Domain.Common.Exceptions.CustomExceptions;

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
        
        var cachedPost = await cache.GetAsync<PostResponseDto>(cacheKey, cancellationToken);
        if (cachedPost is not null) return cachedPost;
        
        var post = await postRepository.GetByIdAsync(guid);
        if (post is null) throw new NotFoundException("Post not found.");
        
        var postResponseDto = mapper.Map<PostResponseDto>(post);
        
        await cache.SetAsync(cacheKey, postResponseDto, TimeSpan.FromMinutes(10), cancellationToken);
        
        return postResponseDto;
    }
}