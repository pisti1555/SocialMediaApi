using Application.Common.Helpers;
using Application.Common.Interfaces.Repositories.Post;
using Application.Responses;
using AutoMapper;
using Cortex.Mediator.Queries;
using Domain.Common.Exceptions.CustomExceptions;

namespace Application.Requests.Posts.PostComment.Queries.GetCommentsOfPost;

public class GetCommentsOfPostHandler(
    IPostRepository postRepository,
    IMapper mapper
) : IQueryHandler<GetCommentsOfPostQuery, List<PostCommentResponseDto>>
{
    public async Task<List<PostCommentResponseDto>> Handle(GetCommentsOfPostQuery request, CancellationToken cancellationToken)
    {
        var guid = Parser.ParseIdOrThrow(request.PostId);
        
        var isPostExists = await postRepository.ExistsAsync(guid);
        if (!isPostExists) throw new NotFoundException("Post not found.");

        var comments = await postRepository.CommentRepository.GetAllCommentOfPostAsync(guid);

        return mapper.Map<List<PostCommentResponseDto>>(comments);
    }
}