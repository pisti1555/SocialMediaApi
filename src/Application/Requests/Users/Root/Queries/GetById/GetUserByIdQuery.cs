using Application.Responses;
using Cortex.Mediator.Queries;

namespace Application.Requests.Users.Root.Queries.GetById;

public record GetUserByIdQuery(string Id) : IQuery<UserResponseDto>;