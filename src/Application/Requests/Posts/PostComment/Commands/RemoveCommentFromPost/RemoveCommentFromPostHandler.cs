using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories.Post;
using Cortex.Mediator;
using Cortex.Mediator.Commands;
using Domain.Common.Exceptions.CustomExceptions;

namespace Application.Requests.Posts.PostComment.Commands.RemoveCommentFromPost;

public class RemoveCommentFromPostHandler(IPostRepository postRepository) : ICommandHandler<RemoveCommentFromPostCommand, Unit>
{
    public async Task<Unit> Handle(RemoveCommentFromPostCommand request, CancellationToken cancellationToken)
    {
        var postGuid = Parser.ParseIdOrThrow(request.PostId);
        var userGuid = Parser.ParseIdOrThrow(request.UserId);
        var commentGuid = Parser.ParseIdOrThrow(request.CommentId);
        
        var post = await postRepository.GetByIdAsync(postGuid);
        if (post is null) throw new BadRequestException("Post not found.");
        
        var comment = await postRepository.CommentRepository.GetByIdAsync(commentGuid);
        if (comment is null) throw new BadRequestException("Comment not found.");
        
        if (comment.PostId != postGuid) throw new BadRequestException("Post does not own the comment.");
        if (comment.UserId != userGuid) throw new BadRequestException("User does not own the comment.");
        
        post.UpdateLastInteraction();
        
        postRepository.CommentRepository.Delete(comment);
        postRepository.Update(post);

        if (!await postRepository.SaveChangesAsync())
            throw new BadRequestException("Comment could not be deleted.");
        
        return Unit.Value;
    }
}