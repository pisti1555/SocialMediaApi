using Application.Responses;
using Cortex.Mediator.Commands;

namespace Application.Requests.Auth.Commands.Login;

public record LoginCommand(
    string UserName, 
    string Password,
    bool RememberMe
) : ICommand<AuthenticatedUserResponseDto>;