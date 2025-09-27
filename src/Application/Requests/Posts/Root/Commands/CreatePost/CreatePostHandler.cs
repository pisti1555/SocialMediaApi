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

namespace Application.Requests.Posts.Root.Commands.CreatePost;

public class CreatePostHandler(
    IRepository<Post, PostResponseDto> postRepository,
    IRepository<AppUser, UserResponseDto> userRepository,
    ICacheService cache,
    IMapper mapper
) : ICommandHandler<CreatePostCommand, PostResponseDto>
{
    public async Task<PostResponseDto> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var guid = Parser.ParseIdOrThrow(request.UserId);
        var user = await userRepository.GetEntityByIdAsync(guid);
        if (user is null) throw new BadRequestException("User not found.");

        var post = PostFactory.Create(request.Text, user);
        
        postRepository.Add(post);
        
        if (!await postRepository.SaveChangesAsync(cancellationToken))
            throw new BadRequestException("Post could not be created.");
        
        var postResponseDto = mapper.Map<PostResponseDto>(post);
        
        await cache.SetAsync($"post-{post.Id.ToString()}", postResponseDto, cancellationToken);
        
        return postResponseDto;
    }
}