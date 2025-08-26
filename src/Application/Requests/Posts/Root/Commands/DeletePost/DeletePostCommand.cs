using Cortex.Mediator;
using Cortex.Mediator.Commands;

namespace Application.Requests.Posts.Root.Commands.DeletePost;

public record DeletePostCommand(string PostId) : ICommand<Unit>;