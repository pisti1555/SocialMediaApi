using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories;
using Application.Contracts.Services;
using Application.Responses;
using Cortex.Mediator.Queries;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using XComment = Domain.Posts.PostComment;

namespace Application.Requests.Posts.PostComment.Queries.GetAllOfPost;

public class GetCommentsOfPostHandler(
    IRepository<Post, PostResponseDto> postRepository,
    IRepository<XComment, PostCommentResponseDto> commentRepository,
    ICacheService cache
) : IQueryHandler<GetCommentsOfPostQuery, List<PostCommentResponseDto>>
{
    public async Task<List<PostCommentResponseDto>> Handle(GetCommentsOfPostQuery request, CancellationToken cancellationToken)
    {
        var postGuid = Parser.ParseIdOrThrow(request.PostId);
        
        var isPostExists = await postRepository.ExistsAsync(postGuid, cancellationToken);
        if (!isPostExists) throw new NotFoundException("Post not found.");
        
        var cacheKey = $"post-comments-{request.PostId}";
        var cachedList = await cache.GetAsync<List<PostCommentResponseDto>>(cacheKey, cancellationToken);
        if (cachedList is not null) return cachedList;

        var comments = await commentRepository.GetAllAsync(x => x.PostId == postGuid, cancellationToken);
        
        await cache.SetAsync(cacheKey, comments, TimeSpan.FromMinutes(10), cancellationToken);

        return comments;
    }
}