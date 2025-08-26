using Application.Responses;
using Cortex.Mediator.Commands;

namespace Application.Requests.Posts.PostComment.Commands.AddCommentToPost;

public record AddCommentToPostCommand(string PostId, string UserId, string Text) : ICommand<PostCommentResponseDto>;