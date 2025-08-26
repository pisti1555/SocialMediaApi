using Application.Responses;
using Cortex.Mediator.Queries;

namespace Application.Requests.Posts.PostLike.Queries.GetLikesOfPost;

public record GetLikesOfPostQuery(string PostId) : IQuery<List<PostLikeResponseDto>>;