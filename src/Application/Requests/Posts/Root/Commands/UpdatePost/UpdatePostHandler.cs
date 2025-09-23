using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories;
using Application.Contracts.Services;
using Application.Responses;
using AutoMapper;
using Cortex.Mediator.Commands;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Domain.Users;

namespace Application.Requests.Posts.Root.Commands.UpdatePost;

public class UpdatePostHandler(
    IRepository<AppUser, UserResponseDto> userRepository,
    IRepository<Post, PostResponseDto> postRepository,
    ICacheService cache,
    IMapper mapper
) : ICommandHandler<UpdatePostCommand, PostResponseDto>
{
    public async Task<PostResponseDto> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var userId = Parser.ParseIdOrThrow(request.UserId);
        var postId = Parser.ParseIdOrThrow(request.PostId);
        
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null) throw new BadRequestException("User not found.");
        
        var post = await postRepository.GetEntityByIdAsync(postId);
        if (post is null) throw new NotFoundException("Post not found.");
        
        if (post.UserId != userId) throw new BadRequestException("User does not own the post.");
        
        post.UpdateText(request.Text);
        
        postRepository.Update(post);
        
        if (!await postRepository.SaveChangesAsync(cancellationToken))
            throw new BadRequestException("Post could not be updated.");
        
        var postResponseDto = mapper.Map<PostResponseDto>(post);
        
        await cache.SetAsync($"post-{post.Id.ToString()}", postResponseDto, cancellationToken);
        
        return postResponseDto;
    }
}