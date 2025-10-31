using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories;
using Application.Contracts.Services;
using Application.Responses;
using Cortex.Mediator;
using Cortex.Mediator.Commands;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;

namespace Application.Requests.Posts.Root.Commands.DeletePost;

public class DeletePostHandler(
    IRepository<Post> repository,
    ICacheService cache
) : ICommandHandler<DeletePostCommand, Unit>
{
    public async Task<Unit> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var postId = Parser.ParseIdOrThrow(request.PostId);
        var userId = Parser.ParseIdOrThrow(request.UserId);
        
        var post = await repository.GetByIdAsync(postId, cancellationToken);
        if (post is null) throw new NotFoundException("Post not found.");
        
        var exists = await repository.ExistsAsync(x => x.UserId == userId && x.Id == postId, cancellationToken);
        if (!exists) throw new BadRequestException("User does not own the post.");
        
        repository.Delete(post);

        if (!await repository.SaveChangesAsync(cancellationToken))
            throw new BadRequestException("Post could not be deleted.");
        
        await cache.RemoveAsync($"post-{postId.ToString()}", cancellationToken);
        
        return Unit.Value;
    }
}