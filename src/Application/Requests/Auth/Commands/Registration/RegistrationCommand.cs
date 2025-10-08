using Application.Responses;
using Cortex.Mediator.Commands;

namespace Application.Requests.Auth.Commands.Registration;

public record RegistrationCommand
(
    string UserName, 
    string Email, 
    string Password,
    string FirstName, 
    string LastName, 
    string DateOfBirth,
    bool RememberMe
) : ICommand<AuthenticatedUserResponseDto>;