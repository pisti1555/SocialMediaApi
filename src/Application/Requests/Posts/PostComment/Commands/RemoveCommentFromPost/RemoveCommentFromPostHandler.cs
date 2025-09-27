using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories;
using Application.Contracts.Services;
using Application.Responses;
using Cortex.Mediator;
using Cortex.Mediator.Commands;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using XComment = Domain.Posts.PostComment;

namespace Application.Requests.Posts.PostComment.Commands.RemoveCommentFromPost;

public class RemoveCommentFromPostHandler(
    IRepository<Post, PostResponseDto> postRepository,
    IRepository<XComment, PostCommentResponseDto> commentRepository,
    ICacheService cache
    ) : ICommandHandler<RemoveCommentFromPostCommand, Unit>
{
    public async Task<Unit> Handle(RemoveCommentFromPostCommand request, CancellationToken cancellationToken)
    {
        var postGuid = Parser.ParseIdOrThrow(request.PostId);
        var userGuid = Parser.ParseIdOrThrow(request.UserId);
        var commentGuid = Parser.ParseIdOrThrow(request.CommentId);
        
        var post = await postRepository.GetEntityByIdAsync(postGuid);
        if (post is null) throw new NotFoundException("Post not found.");
        
        var comment = await commentRepository.GetEntityByIdAsync(commentGuid);
        if (comment is null) throw new NotFoundException("Comment not found.");
        
        if (comment.PostId != postGuid) throw new BadRequestException("Post does not own the comment.");
        if (comment.UserId != userGuid) throw new BadRequestException("User does not own the comment.");
        
        post.UpdateLastInteraction();
        
        commentRepository.Delete(comment);
        postRepository.Update(post);

        if (!await postRepository.SaveChangesAsync(cancellationToken))
            throw new BadRequestException("Comment could not be deleted.");
        
        await cache.RemoveAsync($"post-comments-{postGuid.ToString()}", cancellationToken);
        
        return Unit.Value;
    }
}