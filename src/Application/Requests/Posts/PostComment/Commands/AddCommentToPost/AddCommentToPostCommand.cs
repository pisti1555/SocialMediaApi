using MediatR;

namespace Application.Requests.Posts.PostComment.Commands.AddCommentToPost;

public record AddCommentToPostCommand(string PostId, string UserId, string Text) : IRequest;