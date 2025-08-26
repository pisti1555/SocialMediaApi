using Cortex.Mediator;
using Cortex.Mediator.Commands;

namespace Application.Requests.Posts.PostLike.Commands.DislikePost;

public record DislikePostCommand(string PostId, string UserId) : ICommand<Unit>;