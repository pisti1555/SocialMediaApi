using Application.Common.Interfaces.Repositories;
using Domain.Posts;
using Domain.Posts.Factories;
using Domain.Users;
using MediatR;
using Shared.Exceptions.CustomExceptions;

namespace Application.Requests.Posts.PostLike.Commands.LikePost;

public class LikePostHandler(
        IPostRepository postRepository, IAppUserRepository userRepository
    ) : IRequestHandler<LikePostCommand>
{
    public async Task Handle(LikePostCommand request, CancellationToken cancellationToken)
    {
        var postGuid = ParseGuid(request.PostId);
        var userGuid = ParseGuid(request.UserId);

        var post = await GetPostById(postGuid);
        var user = await GetUserById(userGuid);
        
        var like = PostLikeFactory.Create(user, post);
        post.Likes.Add(like);
        
        if (!await postRepository.SaveChangesAsync())
            throw new BadRequestException("Like could not be created.");
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
    private async Task<Post> GetPostById(Guid postId)
    {
        var post = await postRepository.GetByIdAsync(postId);
        if (post is null)
            throw new NotFoundException("Post not found.");
        return post;
    } 
}