using Application.Responses;
using Cortex.Mediator.Queries;

namespace Application.Requests.Posts.PostLike.Queries.GetAllOfPost;

public record GetLikesOfPostQuery(string PostId) : IQuery<List<PostLikeResponseDto>>;