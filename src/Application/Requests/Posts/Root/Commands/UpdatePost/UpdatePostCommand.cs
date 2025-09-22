using Application.Responses;
using Cortex.Mediator.Commands;

namespace Application.Requests.Posts.Root.Commands.UpdatePost;

public record UpdatePostCommand(string PostId, string UserId, string Text) : ICommand<PostResponseDto>;