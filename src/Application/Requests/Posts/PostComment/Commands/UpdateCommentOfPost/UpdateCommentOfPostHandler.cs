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
    IRepository<AppUser, UserResponseDto> userRepository,
    IRepository<Post, PostResponseDto> postRepository,
    IRepository<XComment, PostCommentResponseDto> commentRepository,
    ICacheService cache,
    IMapper mapper
) : ICommandHandler<UpdateCommentOfPostCommand, PostCommentResponseDto>
{
    public async Task<PostCommentResponseDto> Handle(UpdateCommentOfPostCommand request, CancellationToken cancellationToken)
    {
        var userId = Parser.ParseIdOrThrow(request.UserId);
        var postId = Parser.ParseIdOrThrow(request.PostId);
        var commentId = Parser.ParseIdOrThrow(request.CommentId);
        
        var user = await userRepository.GetEntityByIdAsync(userId);
        if (user is null) throw new BadRequestException("User not found.");

        var post = await postRepository.GetEntityByIdAsync(postId);
        if (post is null) throw new NotFoundException("Post not found.");
        
        var comment = await commentRepository.GetEntityByIdAsync(commentId);
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