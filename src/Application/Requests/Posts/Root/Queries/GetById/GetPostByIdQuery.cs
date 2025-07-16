using Application.Responses;
using MediatR;

namespace Application.Requests.Posts.Root.Queries.GetById;

public record GetPostByIdQuery(string Id) : IRequest<PostResponseDto>;