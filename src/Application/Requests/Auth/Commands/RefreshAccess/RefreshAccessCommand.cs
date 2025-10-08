using Application.Responses;
using Cortex.Mediator.Commands;

namespace Application.Requests.Auth.Commands.RefreshAccess;

public record RefreshAccessCommand(
    string AccessToken, 
    string RefreshToken
) : ICommand<TokenResponseDto>;