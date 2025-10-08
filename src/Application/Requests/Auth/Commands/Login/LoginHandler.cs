using Application.Contracts.Persistence.Repositories;
using Application.Contracts.Services;
using Application.Responses;
using AutoMapper;
using Cortex.Mediator.Commands;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;

namespace Application.Requests.Auth.Commands.Login;

public class LoginHandler(
    IRepository<AppUser, UserResponseDto> repository, 
    IAuthService authService,
    ITokenService tokenService,
    IMapper mapper
) : ICommandHandler<LoginCommand, AuthenticatedUserResponseDto>
{
    public async Task<AuthenticatedUserResponseDto> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await GetUserAsync(command.UserName, cancellationToken);
        await ThrowIfInvalidCredentials(user, command.Password, cancellationToken);

        var roles = await authService.GetRolesAsync(user, cancellationToken);
        
        var accessToken = tokenService.CreateAccessToken(
            uid: user.Id.ToString(), 
            name: user.UserName, 
            email: user.Email,
            roles: roles,
            sid: null
        );
        var refreshToken = tokenService.CreateRefreshToken();

        await authService.SaveTokenAsync(accessToken, refreshToken, command.RememberMe, cancellationToken);
        
        var responseDto = mapper.Map<AuthenticatedUserResponseDto>(user);
        responseDto.AccessToken = accessToken;
        responseDto.RefreshToken = refreshToken;
        
        return responseDto;
    }
    
    // Helpers
    private async Task<AppUser> GetUserAsync(string userName, CancellationToken cancellationToken)
    {
        var user = await repository.GetEntityAsync(x => x.UserName == userName, cancellationToken);
        return user ?? throw new UnauthorizedException("Invalid username or password.");
    }
    
    private async Task ThrowIfInvalidCredentials(AppUser user, string password, CancellationToken cancellationToken)
    {
        var result = await authService.CheckPasswordAsync(user, password, cancellationToken);
        if (!result) throw new UnauthorizedException("Invalid username or password.");
    }
}