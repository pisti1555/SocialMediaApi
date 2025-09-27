using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories;
using Application.Contracts.Services;
using Application.Responses;
using Cortex.Mediator;
using Cortex.Mediator.Commands;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Domain.Users;
using XPostLike = Domain.Posts.PostLike;

namespace Application.Requests.Posts.PostLike.Commands.DislikePost;

public class DislikePostHandler(
    IRepository<AppUser, UserResponseDto> userRepository,
    IRepository<Post, PostResponseDto> postRepository,
    IRepository<XPostLike, PostLikeResponseDto> likeRepository,
    ICacheService cache
) : ICommandHandler<DislikePostCommand, Unit>
{ 
    public async Task<Unit> Handle(DislikePostCommand request, CancellationToken cancellationToken)
    {
        var userGuid = Parser.ParseIdOrThrow(request.UserId);
        var postGuid = Parser.ParseIdOrThrow(request.PostId);
        var likeGuid = Parser.ParseIdOrThrow(request.LikeId);
        
        var like = await likeRepository.GetEntityByIdAsync(likeGuid);
        if (like is null) throw new NotFoundException("Like not found.");
        
        if (like.UserId != userGuid) throw new BadRequestException("User does not own the like.");
        if (like.PostId != postGuid) throw new BadRequestException("Post does not own the like.");
        
        var user = await userRepository.GetEntityByIdAsync(userGuid);
        if (user is null) throw new BadRequestException("User not found.");
        
        var post = await postRepository.GetEntityByIdAsync(postGuid);
        if (post is null) throw new NotFoundException("Post not found.");
        
        post.UpdateLastInteraction();
        
        likeRepository.Delete(like);
        postRepository.Update(post);

        if (!await postRepository.SaveChangesAsync(cancellationToken))
            throw new BadRequestException("Like could not be deleted.");
        
        await cache.RemoveAsync($"post-likes-{postGuid.ToString()}", cancellationToken);
        
        return Unit.Value;
    }
}