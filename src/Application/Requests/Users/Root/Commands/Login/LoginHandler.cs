using Application.Contracts.Persistence.Repositories;
using Application.Contracts.Services;
using Application.Responses;
using AutoMapper;
using Cortex.Mediator.Commands;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;

namespace Application.Requests.Users.Root.Commands.Login;

public class LoginHandler(
    IRepository<AppUser, UserResponseDto> repository, 
    IAuthService authService,
    ITokenService tokenService,
    IMapper mapper
) : ICommandHandler<LoginCommand, AuthenticatedUserResponseDto>
{
    public async Task<AuthenticatedUserResponseDto> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await repository.GetEntityAsync(x => x.UserName == command.UserName, cancellationToken);
        if (user is null)
        {
            throw new UnauthorizedException("Invalid username or password.");
        }

        var result = await authService.CheckPasswordAsync(user, command.Password, cancellationToken);
        if (!result)
        {
            throw new UnauthorizedException("Invalid username or password.");
        }
        
        var roles = await authService.GetRolesAsync(user, cancellationToken);

        var token = tokenService.CreateToken(user, roles);
        
        var responseDto = mapper.Map<AuthenticatedUserResponseDto>(user);
        responseDto.Token = token;
        
        return responseDto;
    }
}