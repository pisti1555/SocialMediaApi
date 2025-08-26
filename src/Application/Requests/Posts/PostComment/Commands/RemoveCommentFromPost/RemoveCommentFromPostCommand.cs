using Cortex.Mediator;
using Cortex.Mediator.Commands;

namespace Application.Requests.Posts.PostComment.Commands.RemoveCommentFromPost;

public record RemoveCommentFromPostCommand(string PostId, string CommentId, string UserId) : ICommand<Unit>;