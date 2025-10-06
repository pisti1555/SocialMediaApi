using Application.Responses;
using Cortex.Mediator.Commands;

namespace Application.Requests.Users.Root.Commands.Login;

public record LoginCommand(
    string UserName, 
    string Password
) : ICommand<AuthenticatedUserResponseDto>;