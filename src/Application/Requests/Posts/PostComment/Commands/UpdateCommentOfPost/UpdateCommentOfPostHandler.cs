using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories;
using Application.Contracts.Services;
using Application.Responses;
using AutoMapper;
using Cortex.Mediator.Commands;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Posts;
using Domain.Users;
using XComment = Domain.Posts.PostComment;

namespace Application.Requests.Posts.PostComment.Commands.UpdateCommentOfPost;

public class UpdateCommentOfPostHandler(
    IRepository<AppUser> userRepository,
    IRepository<Post> postRepository,
    IRepository<XComment> commentRepository,
    ICacheService cache,
    IMapper mapper
) : ICommandHandler<UpdateCommentOfPostCommand, PostCommentResponseDto>
{
    public async Task<PostCommentResponseDto> Handle(UpdateCommentOfPostCommand request, CancellationToken cancellationToken)
    {
        var userId = Parser.ParseIdOrThrow(request.UserId);
        var postId = Parser.ParseIdOrThrow(request.PostId);
        var commentId = Parser.ParseIdOrThrow(request.CommentId);
        
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null) throw new BadRequestException("User not found.");

        var post = await postRepository.GetByIdAsync(postId, cancellationToken);
        if (post is null) throw new NotFoundException("Post not found.");
        
        var comment = await commentRepository.GetByIdAsync(commentId, cancellationToken);
        if (comment is null) throw new NotFoundException("Comment not found.");
        
        if (comment.UserId != userId) throw new BadRequestException("User does not own the comment.");
        if (comment.PostId != postId) throw new BadRequestException("Post does not own the comment.");
        
        comment.UpdateText(request.Text, post);
        
        commentRepository.Update(comment);
        
        if (!await postRepository.SaveChangesAsync(cancellationToken))
            throw new BadRequestException("Comment could not be updated.");
        
        var postCommentResponseDto = mapper.Map<PostCommentResponseDto>(comment);
        
        await cache.RemoveAsync($"post-comments-{post.Id.ToString()}", cancellationToken);
        
        return postCommentResponseDto;
    }
}