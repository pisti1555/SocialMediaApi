using MediatR;

namespace Application.Requests.Posts.PostComment.Commands.RemoveCommentFromPost;

public record RemoveCommentFromPostCommand(string PostId, string CommentId, string UserId) : IRequest;