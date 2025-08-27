using Application.Common.Helpers;
using Application.Common.Interfaces.Persistence.Repositories.Post;
using Cortex.Mediator;
using Cortex.Mediator.Commands;
using Domain.Common.Exceptions.CustomExceptions;

namespace Application.Requests.Posts.PostLike.Commands.DislikePost;

public class DislikePostHandler(
    IPostRepository postRepository
) : ICommandHandler<DislikePostCommand, Unit>
{ 
    public async Task<Unit> Handle(DislikePostCommand request, CancellationToken cancellationToken)
    {
        var userGuid = Parser.ParseIdOrThrow(request.UserId);
        var postGuid = Parser.ParseIdOrThrow(request.PostId);
        
        var like = await postRepository.LikeRepository.GetByUserIdAndPostIdAsync(userGuid, postGuid);
        if (like is null) throw new BadRequestException("Like not found.");
        
        if (like.UserId != userGuid) throw new BadRequestException("User does not own the like.");
        if (like.PostId != postGuid) throw new BadRequestException("Post does not own the like.");
        
        var post = await postRepository.GetByIdAsync(postGuid);
        if (post is null) throw new BadRequestException("Post not found.");
        
        post.UpdateLastInteraction();
        
        postRepository.LikeRepository.Delete(like);
        postRepository.Update(post);

        if (!await postRepository.SaveChangesAsync())
            throw new BadRequestException("Like could not be deleted.");
        
        return Unit.Value;
    }
}