using Application.Responses;
using Cortex.Mediator.Commands;

namespace Application.Requests.Posts.PostComment.Commands.UpdateCommentOfPost;

public record UpdateCommentOfPostCommand(
    string PostId,
    string CommentId, 
    string UserId, 
    string Text
) : ICommand<PostCommentResponseDto>;