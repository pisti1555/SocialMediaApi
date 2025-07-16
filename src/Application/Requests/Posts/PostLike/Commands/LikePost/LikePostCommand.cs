using MediatR;

namespace Application.Requests.Posts.PostLike.Commands.LikePost;

public record LikePostCommand(string UserId, string PostId) : IRequest;