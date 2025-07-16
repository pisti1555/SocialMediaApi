using Application.Responses;
using MediatR;

namespace Application.Requests.Users.Root.Commands.CreateUser;

public record CreateUserCommand
(
    string UserName, string Email, string FirstName, string LastName, DateOnly DateOfBirth
) : IRequest<UserResponseDto>;