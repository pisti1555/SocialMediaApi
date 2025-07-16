using Application.Common.Interfaces.Repositories;
using Domain.Posts;
using Domain.Posts.Factories;
using Domain.Users;
using MediatR;
using Shared.Exceptions.CustomExceptions;

namespace Application.Requests.Posts.PostComment.Commands.AddCommentToPost;

public class AddCommentToPostHandler(
        IPostRepository postRepository, IAppUserRepository userRepository
    ) : IRequestHandler<AddCommentToPostCommand>
{
    public async Task Handle(AddCommentToPostCommand request, CancellationToken cancellationToken)
    {
        var userGuid = ParseGuid(request.UserId);
        var postGuid = ParseGuid(request.PostId);
        
        var user = await GetUserById(userGuid);
        var post = await GetPostById(postGuid);
        
        var comment = PostCommentFactory.Create(request.Text, user, post);
        
        post.Comments.Add(comment);
        
        if (!await postRepository.SaveChangesAsync())
            throw new BadRequestException("Comment could not be created.");
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