using Application.Responses;
using MediatR;

namespace Application.Requests.Posts.Root.Commands.CreatePost;

public record CreatePostCommand(string Text, string UserId) : IRequest<PostResponseDto>;
