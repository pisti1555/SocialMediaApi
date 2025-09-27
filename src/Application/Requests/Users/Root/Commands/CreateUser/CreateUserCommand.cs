using Application.Responses;
using Cortex.Mediator.Commands;

namespace Application.Requests.Users.Root.Commands.CreateUser;

public record CreateUserCommand
(
    string UserName, string Email, string FirstName, string LastName, string DateOfBirth
) : ICommand<UserResponseDto>;