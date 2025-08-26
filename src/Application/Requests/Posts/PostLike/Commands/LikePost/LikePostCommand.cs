using Application.Responses;
using Cortex.Mediator.Commands;

namespace Application.Requests.Posts.PostLike.Commands.LikePost;

public record LikePostCommand(string UserId, string PostId) : ICommand<PostLikeResponseDto>;