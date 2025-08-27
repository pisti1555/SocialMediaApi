using Application.Common.Helpers;
using Application.Common.Interfaces.Persistence.Repositories.AppUser;
using Application.Common.Interfaces.Persistence.Repositories.Post;
using Application.Responses;
using AutoMapper;
using Cortex.Mediator.Commands;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts.Factories;

namespace Application.Requests.Posts.Root.Commands.CreatePost;

public class CreatePostHandler(
    IPostRepository postRepository,
    IAppUserRepository userRepository,
    IMapper mapper
) : ICommandHandler<CreatePostCommand, PostResponseDto>
{
    public async Task<PostResponseDto> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var guid = Parser.ParseIdOrThrow(request.UserId);
        var user = await userRepository.GetByIdAsync(guid);
        if (user is null) throw new BadRequestException("User not found.");

        var post = PostFactory.Create(request.Text, user);
        
        postRepository.Add(post);
        
        if (!await postRepository.SaveChangesAsync())
            throw new BadRequestException("Post could not be created.");
        
        return mapper.Map<PostResponseDto>(post);
    }
}