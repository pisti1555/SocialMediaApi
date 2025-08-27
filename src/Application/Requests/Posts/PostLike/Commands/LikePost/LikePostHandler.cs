using Application.Common.Helpers;
using Application.Common.Interfaces.Persistence.Repositories.AppUser;
using Application.Common.Interfaces.Persistence.Repositories.Post;
using Application.Responses;
using AutoMapper;
using Cortex.Mediator.Commands;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts.Factories;

namespace Application.Requests.Posts.PostLike.Commands.LikePost;

public class LikePostHandler(
    IPostRepository postRepository,
    IAppUserRepository userRepository,
    IMapper mapper
) : ICommandHandler<LikePostCommand, PostLikeResponseDto>
{
    public async Task<PostLikeResponseDto> Handle(LikePostCommand request, CancellationToken cancellationToken)
    {
        var postGuid = Parser.ParseIdOrThrow(request.PostId);
        var userGuid = Parser.ParseIdOrThrow(request.UserId);

        var post = await postRepository.GetByIdAsync(postGuid);
        if (post is null) throw new BadRequestException("Post not found.");
        
        var user = await userRepository.GetByIdAsync(userGuid);
        if (user is null) throw new BadRequestException("User not found.");

        var hasUserLikedPost = await postRepository.LikeRepository.ExistsAsync(postGuid, userGuid);
        if (hasUserLikedPost) throw new BadRequestException("User already liked post.");
        
        var like = PostLikeFactory.Create(user, post);
        
        post.UpdateLastInteraction();
        
        postRepository.LikeRepository.Add(like);
        postRepository.Update(post);
        
        if (!await postRepository.SaveChangesAsync())
            throw new BadRequestException("Like could not be created.");
        
        return mapper.Map<PostLikeResponseDto>(like);
    }
}