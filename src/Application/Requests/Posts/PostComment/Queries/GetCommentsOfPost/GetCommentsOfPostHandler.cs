using Application.Common.Interfaces.Repositories;
using Application.Responses;
using MediatR;
using Shared.Exceptions.CustomExceptions;

namespace Application.Requests.Posts.PostComment.Queries.GetCommentsOfPost;

public class GetCommentsOfPostHandler(IPostRepository postRepository) : IRequestHandler<GetCommentsOfPostQuery, List<PostCommentResponseDto>>
{
    public async Task<List<PostCommentResponseDto>> Handle(GetCommentsOfPostQuery request, CancellationToken cancellationToken)
    {
        var guid = ParseGuid(request.PostId);
        await ThrowIfPostNotExistsById(guid);

        return await postRepository.GetAllCommentDtoOfPostAsync(guid);
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