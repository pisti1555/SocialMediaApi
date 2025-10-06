using Application.Responses;
using Cortex.Mediator.Commands;

namespace Application.Requests.Users.Root.Commands.CreateUser;

public record CreateUserCommand
(
    string UserName, 
    string Email, 
    string Password,
    string FirstName, 
    string LastName, 
    string DateOfBirth
) : ICommand<AuthenticatedUserResponseDto>;