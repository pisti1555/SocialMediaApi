using Application.Responses;
using MediatR;

namespace Application.Requests.Users.Root.Queries.GetById;

public record GetUserByIdQuery(string Id) : IRequest<UserResponseDto>;