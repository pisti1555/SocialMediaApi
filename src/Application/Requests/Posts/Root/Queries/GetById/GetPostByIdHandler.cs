using Application.Common.Interfaces.Repositories;
using Application.Responses;
using MediatR;
using Shared.Exceptions.CustomExceptions;

namespace Application.Requests.Posts.Root.Queries.GetById;

public class GetPostByIdHandler(IPostRepository postRepository) : IRequestHandler<GetPostByIdQuery, PostResponseDto>
{
    public async Task<PostResponseDto> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var guid = ParseGuid(request.Id);
        
        var post = await postRepository.GetPostDtoByIdAsync(guid);
        if (post is null)
            throw new NotFoundException("Post not found.");
        
        return post;
    }
    
    private static Guid ParseGuid(string id)
    {
        var result = Guid.TryParse(id, out var guid);
        if (!result)
            throw new BadRequestException("Cannot parse the id.");
        return guid;  
    }
}