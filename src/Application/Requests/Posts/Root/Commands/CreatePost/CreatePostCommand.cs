using Application.Responses;
using Cortex.Mediator.Commands;

namespace Application.Requests.Posts.Root.Commands.CreatePost;

public record CreatePostCommand(string Text, string UserId) : ICommand<PostResponseDto>;