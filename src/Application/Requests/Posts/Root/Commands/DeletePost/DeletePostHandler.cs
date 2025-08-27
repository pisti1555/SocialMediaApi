using Application.Common.Helpers;
using Application.Common.Interfaces.Persistence.Repositories.Post;
using Cortex.Mediator;
using Cortex.Mediator.Commands;
using Domain.Common.Exceptions.CustomExceptions;

namespace Application.Requests.Posts.Root.Commands.DeletePost;

public class DeletePostHandler(
    IPostRepository postRepository
) : ICommandHandler<DeletePostCommand, Unit>
{
    public async Task<Unit> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var guid = Parser.ParseIdOrThrow(request.PostId);
        var post = await postRepository.GetByIdAsync(guid);
        if (post is null) throw new BadRequestException("Post not found.");
        
        postRepository.Delete(post);

        if (!await postRepository.SaveChangesAsync())
            throw new BadRequestException("Post could not be deleted.");
        
        return Unit.Value;
    }
}