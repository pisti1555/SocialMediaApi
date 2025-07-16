using MediatR;

namespace Application.Requests.Posts.PostLike.Commands.DislikePost;

public record DislikePostCommand(string PostId, string UserId) : IRequest;