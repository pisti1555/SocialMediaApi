using Application.Responses;
using Cortex.Mediator.Queries;

namespace Application.Requests.Posts.Root.Queries.GetById;

public record GetPostByIdQuery(string Id) : IQuery<PostResponseDto>;