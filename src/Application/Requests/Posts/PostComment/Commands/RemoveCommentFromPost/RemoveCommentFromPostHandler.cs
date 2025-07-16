using Application.Common.Interfaces.Repositories;
using Domain.Posts;
using MediatR;
using Shared.Exceptions.CustomExceptions;

namespace Application.Requests.Posts.PostComment.Commands.RemoveCommentFromPost;

public class RemoveCommentFromPostHandler(IPostRepository postRepository) : IRequestHandler<RemoveCommentFromPostCommand>
{
    public async Task Handle(RemoveCommentFromPostCommand request, CancellationToken cancellationToken)
    {
        var userGuid = ParseGuid(request.UserId);
        var postGuid = ParseGuid(request.PostId);
        var commentGuid = ParseGuid(request.CommentId);
        var post = await GetPostById(postGuid);
        
        post.Comments.Remove(post.Comments.First(x => x.Id == commentGuid && x.UserId == userGuid));
        
        if (!await postRepository.SaveChangesAsync())
            throw new BadRequestException("Comment could not be deleted.");
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