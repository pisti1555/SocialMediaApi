using MediatR;

namespace Application.Requests.Posts.Root.Commands.DeletePost;

public record DeletePostCommand(string PostId) : IRequest<bool>;