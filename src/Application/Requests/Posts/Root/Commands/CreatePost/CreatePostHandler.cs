using Application.Common.Interfaces.Repositories;
using Application.Responses;
using AutoMapper;
using Domain.Posts.Factories;
using Domain.Users;
using MediatR;
using Shared.Exceptions.CustomExceptions;

namespace Application.Requests.Posts.Root.Commands.CreatePost;

public class CreatePostHandler(
        IPostRepository postRepository, IAppUserRepository userRepository, IMapper mapper
    ) : IRequestHandler<CreatePostCommand, PostResponseDto>
{
    public async Task<PostResponseDto> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var guid = ParseGuid(request.UserId);
        var user = await GetUserById(guid);

        var post = PostFactory.Create(request.Text, user);
        
        postRepository.Add(post);
        
        if (!await postRepository.SaveChangesAsync())
            throw new BadRequestException("Post could not be created.");
        
        return mapper.Map<PostResponseDto>(post);
    }
    
    private static Guid ParseGuid(string id)
    {
        var result = Guid.TryParse(id, out var guid);
        if (!result)
            throw new BadRequestException("Cannot parse the id.");
        return guid;
    }
    private async Task<AppUser> GetUserById(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException("User not found.");
        return user;
    }
}