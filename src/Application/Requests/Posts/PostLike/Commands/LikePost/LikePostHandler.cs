using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories;
using Application.Contracts.Services;
using Application.Responses;
using AutoMapper;
using Cortex.Mediator.Commands;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Domain.Posts.Factories;
using Domain.Users;
using XPostLike = Domain.Posts.PostLike;

namespace Application.Requests.Posts.PostLike.Commands.LikePost;

public class LikePostHandler(
    IRepository<Post, PostResponseDto> postRepository,
    IRepository<AppUser, UserResponseDto> userRepository,
    IRepository<XPostLike, PostLikeResponseDto> likeRepository,
    ICacheService cache,
    IMapper mapper
) : ICommandHandler<LikePostCommand, PostLikeResponseDto>
{
    public async Task<PostLikeResponseDto> Handle(LikePostCommand request, CancellationToken cancellationToken)
    {
        var postGuid = Parser.ParseIdOrThrow(request.PostId);
        var userGuid = Parser.ParseIdOrThrow(request.UserId);

        var post = await postRepository.GetEntityByIdAsync(postGuid);
        if (post is null) throw new NotFoundException("Post not found.");
        
        var user = await userRepository.GetEntityByIdAsync(userGuid);
        if (user is null) throw new BadRequestException("User not found.");

        var hasUserLikedPost = await likeRepository.ExistsAsync(x => 
            x.UserId == userGuid && x.PostId == postGuid, 
            cancellationToken
        );
        
        if (hasUserLikedPost) throw new ConflictException("Already liked post.");
        
        var like = PostLikeFactory.Create(user, post);
        
        post.UpdateLastInteraction();
        
        likeRepository.Add(like);
        postRepository.Update(post);
        
        if (!await postRepository.SaveChangesAsync(cancellationToken))
            throw new BadRequestException("Like could not be created.");
        
        await cache.RemoveAsync($"post-likes-{postGuid.ToString()}", cancellationToken);
        
        return mapper.Map<PostLikeResponseDto>(like);
    }
}