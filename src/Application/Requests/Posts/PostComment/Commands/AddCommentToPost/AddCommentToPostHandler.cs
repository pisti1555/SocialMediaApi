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
using XComment = Domain.Posts.PostComment;

namespace Application.Requests.Posts.PostComment.Commands.AddCommentToPost;

public class AddCommentToPostHandler(
    IRepository<AppUser> userRepository,
    IRepository<Post> postRepository,
    IRepository<XComment> commentRepository,
    ICacheService cache,
    IMapper mapper
) : ICommandHandler<AddCommentToPostCommand, PostCommentResponseDto>
{
    public async Task<PostCommentResponseDto> Handle(AddCommentToPostCommand request, CancellationToken cancellationToken)
    {
        var userGuid = Parser.ParseIdOrThrow(request.UserId);
        var postGuid = Parser.ParseIdOrThrow(request.PostId);
        
        var user = await userRepository.GetByIdAsync(userGuid, cancellationToken);
        if (user is null) throw new BadRequestException("User not found.");
        
        var post = await postRepository.GetByIdAsync(postGuid, cancellationToken);
        if (post is null) throw new NotFoundException("Post not found.");
        
        var comment = PostCommentFactory.Create(request.Text, user, post);
        
        post.UpdateLastInteraction();
        
        commentRepository.Add(comment);
        postRepository.Update(post);

        if (!await postRepository.SaveChangesAsync(cancellationToken))
            throw new BadRequestException("Comment could not be created.");
        
        await cache.RemoveAsync($"post-comments-{postGuid.ToString()}", cancellationToken);
        
        return mapper.Map<PostCommentResponseDto>(comment);
    }
}