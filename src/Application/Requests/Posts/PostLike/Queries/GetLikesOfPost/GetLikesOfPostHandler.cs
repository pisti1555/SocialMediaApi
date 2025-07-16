using Application.Common.Interfaces.Repositories;
using Application.Responses;
using MediatR;
using Shared.Exceptions.CustomExceptions;

namespace Application.Requests.Posts.PostLike.Queries.GetLikesOfPost;

public class GetLikesOfPostHandler(IPostRepository postRepository) : IRequestHandler<GetLikesOfPostQuery, List<PostLikeResponseDto>>
{
    public async Task<List<PostLikeResponseDto>> Handle(GetLikesOfPostQuery request, CancellationToken cancellationToken)
    {
        var guid = ParseGuid(request.PostId);
        await ThrowIfPostNotExistsById(guid);

        return await postRepository.GetAllLikeDtoOfPostAsync(guid);
    }
    
    private static Guid ParseGuid(string id)
    {
        var result = Guid.TryParse(id, out var guid);
        if (!result)
            throw new BadRequestException("Cannot parse the id.");
        return guid;
    }
    private async Task ThrowIfPostNotExistsById(Guid postId)
    {
        var exists = await postRepository.ExistsAsync(postId);
        if (!exists)
            throw new NotFoundException("Post not found.");
    }
}