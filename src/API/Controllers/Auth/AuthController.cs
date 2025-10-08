using API.Controllers.Common;
using API.Controllers.User;
using API.DTOs.Bodies.Auth;
using Application.Requests.Auth.Commands.Login;
using Application.Requests.Auth.Commands.RefreshAccess;
using Application.Requests.Auth.Commands.Registration;
using Application.Responses;
using Asp.Versioning;
using Cortex.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Auth;

[ApiVersion(1)]
[Route("api/v{v:ApiVersion}/auth")]
public class AuthController(IMediator mediator) : BaseApiController
{
    [MapToApiVersion(1)]
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticatedUserResponseDto>> Login([FromBody]LoginDto dto, CancellationToken ct)
    {
        var command = new LoginCommand(
            dto.UserName ?? string.Empty, 
            dto.Password ?? string.Empty,
            dto.RememberMe ?? false
        );
        
        var result = await mediator.SendCommandAsync<LoginCommand, AuthenticatedUserResponseDto>(command, ct);
        return result;
    }
    
    [MapToApiVersion(1)]
    [HttpPost("register")]
    public async Task<ActionResult<AuthenticatedUserResponseDto>> Register([FromBody]RegistrationDto dto, CancellationToken ct)
    {
        var command = new RegistrationCommand(
            dto.UserName ?? string.Empty, 
            dto.Email ?? string.Empty, 
            dto.Password ?? string.Empty,
            dto.FirstName ?? string.Empty, 
            dto.LastName ?? string.Empty, 
            dto.DateOfBirth ?? string.Empty,
            dto.RememberMe ?? false
        );
        
        var result = await mediator.SendCommandAsync<RegistrationCommand, AuthenticatedUserResponseDto>(command, ct);
        return CreatedAtAction(
            actionName: nameof(UserController.GetById), 
            controllerName: "User", 
            routeValues: new {id = result.Id},
            value: result
        );
    }

    [MapToApiVersion(1)]
    [HttpPost("refresh-access")]
    public async Task<ActionResult<TokenResponseDto>> RefreshAccess([FromBody]RefreshAccessDto dto, CancellationToken ct)
    {
        var command = new RefreshAccessCommand(
            dto.AccessToken ?? string.Empty,
            dto.RefreshToken ?? string.Empty
        );
        
        var result = await mediator.SendCommandAsync<RefreshAccessCommand, TokenResponseDto>(command, ct);
        return result;
    }
}