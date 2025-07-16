using Application.Common.Interfaces.Repositories;
using Domain.Posts;
using MediatR;
using Shared.Exceptions.CustomExceptions;

namespace Application.Requests.Posts.Root.Commands.DeletePost;

public class DeletePostHandler(IPostRepository postRepository) : IRequestHandler<DeletePostCommand, bool>
{
    public async Task<bool> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var guid = ParseGuid(request.PostId);
        var post = await GetPostById(guid);
        postRepository.Delete(post);
        if (!await postRepository.SaveChangesAsync())
            throw new BadRequestException("Post could not be deleted.");

        return true;
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