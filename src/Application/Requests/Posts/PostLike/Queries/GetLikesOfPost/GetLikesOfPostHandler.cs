using Application.Common.Helpers;
using Application.Common.Interfaces.Persistence.Repositories.Post;
using Application.Responses;
using AutoMapper;
using Cortex.Mediator.Queries;
using Domain.Common.Exceptions.CustomExceptions;

namespace Application.Requests.Posts.PostLike.Queries.GetLikesOfPost;

public class GetLikesOfPostHandler(
    IPostRepository postRepository,
    IMapper mapper
) : IQueryHandler<GetLikesOfPostQuery, List<PostLikeResponseDto>>
{
    public async Task<List<PostLikeResponseDto>> Handle(GetLikesOfPostQuery request, CancellationToken cancellationToken)
    {
        var guid = Parser.ParseIdOrThrow(request.PostId);
        if (!await postRepository.ExistsAsync(guid)) throw new NotFoundException("Post not found.");
        
        var result = await postRepository.LikeRepository.GetAllOfPostAsync(guid);
        
        return mapper.Map<List<PostLikeResponseDto>>(result);
    }
}