using Application.Responses;
using MediatR;

namespace Application.Requests.Posts.PostLike.Queries.GetLikesOfPost;

public record GetLikesOfPostQuery(string PostId) : IRequest<List<PostLikeResponseDto>>;