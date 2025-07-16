using Application.Common.Interfaces.Repositories;
using Domain.Posts;
using MediatR;
using Shared.Exceptions.CustomExceptions;

namespace Application.Requests.Posts.PostLike.Commands.DislikePost;

public class DislikePostHandler(IPostRepository postRepository) : IRequestHandler<DislikePostCommand>
{
    public async Task Handle(DislikePostCommand request, CancellationToken cancellationToken)
    {
        var userGuid = ParseGuid(request.UserId);
        var postGuid = ParseGuid(request.PostId);
        
        var post = await GetPostById(postGuid);
        
        post.Likes.Remove(post.Likes.First(x => x.UserId == userGuid));
        
        if (!await postRepository.SaveChangesAsync())
            throw new BadRequestException("Like could not be deleted.");
    }
    
    private static Guid ParseGuid(string id)
    {
        var result = Guid.TryParse(id, out var guid);
        if (!result)
            throw new BadRequestException("Cannot parse the id.");
        return guid;
    }
    private async Task<Post> GetPostById(Guid postId)
    {
        var post = await postRepository.GetByIdAsync(postId);
        if (post is null)
            throw new NotFoundException("Post not found.");
        return post;
    } 
}