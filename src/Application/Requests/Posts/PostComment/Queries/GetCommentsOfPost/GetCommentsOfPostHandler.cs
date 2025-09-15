using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories.Post;
using Application.Contracts.Services;
using Application.Responses;
using AutoMapper;
using Cortex.Mediator.Queries;
using Domain.Common.Exceptions.CustomExceptions;

namespace Application.Requests.Posts.PostComment.Queries.GetCommentsOfPost;

public class GetCommentsOfPostHandler(
    IPostRepository postRepository,
    ICacheService cache,
    IMapper mapper
) : IQueryHandler<GetCommentsOfPostQuery, List<PostCommentResponseDto>>
{
    public async Task<List<PostCommentResponseDto>> Handle(GetCommentsOfPostQuery request, CancellationToken cancellationToken)
    {
        var guid = Parser.ParseIdOrThrow(request.PostId);
        
        var isPostExists = await postRepository.ExistsAsync(guid);
        if (!isPostExists) throw new NotFoundException("Post not found.");
        
        var cacheKey = $"post-comments-{request.PostId}";
        var cachedList = await cache.GetAsync<List<PostCommentResponseDto>>(cacheKey, cancellationToken);
        if (cachedList is not null) return cachedList;

        var comments = await postRepository.CommentRepository.GetAllCommentOfPostAsync(guid);
        await cache.SetAsync(cacheKey, comments.ToList(), TimeSpan.FromMinutes(10), cancellationToken);

        return mapper.Map<List<PostCommentResponseDto>>(comments);
    }
}