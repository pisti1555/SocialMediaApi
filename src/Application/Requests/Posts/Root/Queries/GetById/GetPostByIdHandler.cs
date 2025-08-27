using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories.Post;
using Application.Responses;
using AutoMapper;
using Cortex.Mediator.Queries;
using Domain.Common.Exceptions.CustomExceptions;

namespace Application.Requests.Posts.Root.Queries.GetById;

public class GetPostByIdHandler(IPostRepository postRepository, IMapper mapper) : IQueryHandler<GetPostByIdQuery, PostResponseDto>
{
    public async Task<PostResponseDto> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var guid = Parser.ParseIdOrThrow(request.Id);
        
        var post = await postRepository.GetByIdAsync(guid);
        return post is null ? 
            throw new NotFoundException("Post not found.") : mapper.Map<PostResponseDto>(post);
    }
}