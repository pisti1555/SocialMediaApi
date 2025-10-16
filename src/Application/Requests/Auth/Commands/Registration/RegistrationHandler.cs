using Application.Common.Helpers;
using Application.Contracts.Persistence.Repositories;
using Application.Contracts.Services;
using Application.Responses;
using AutoMapper;
using Cortex.Mediator.Commands;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;
using Domain.Users.Factories;
using FluentValidation;
using FluentValidation.Results;

namespace Application.Requests.Auth.Commands.Registration;

public class RegistrationHandler(
    IRepository<AppUser, UserResponseDto> repository,
    ITokenService tokenService,
    IAuthService authService,
    IHasher hasher,
    IMapper mapper
) : ICommandHandler<RegistrationCommand, AuthenticatedUserResponseDto>
{
    public async Task<AuthenticatedUserResponseDto> Handle(RegistrationCommand request, CancellationToken cancellationToken)
    {
        await ThrowIfUserAlreadyExistsByUsername(request.UserName);
        await ThrowIfUserAlreadyExistsByEmail(request.Email);
        
        var dob = Parser.ParseDateOrThrow(request.DateOfBirth);
        
        var user = AppUserFactory.Create(
            request.UserName, request.Email, request.FirstName, request.LastName, dob
        );

        var createAuthUserResult = await authService.CreateIdentityUserAsync(user, request.Password, cancellationToken);
        if (!createAuthUserResult.Succeeded)
        {
            throw new ValidationException(createAuthUserResult.Errors.Select(x => new ValidationFailure("", x)));
        }
        
        repository.Add(user);

        if (!await repository.SaveChangesAsync(cancellationToken))
        {
            await authService.DeleteIdentityUserAsync(user);
            throw new BadRequestException("User could not be created.");
        }
        
        var roles = await authService.GetRolesAsync(user, cancellationToken);
        var accessToken = tokenService.CreateAccessToken(
            uid: user.Id.ToString(), 
            name: user.UserName, 
            email: user.Email, 
            roles: roles, 
            sid: null
        );
        var refreshToken = tokenService.CreateRefreshToken();
        
        var claims = tokenService.GetValidatedClaimsFromToken(accessToken).Data;

        var tokenResult = await authService.SaveTokenAsync(
            jtiHash: hasher.CreateHash(claims.Jti),  
            refreshTokenHash: hasher.CreateHash(refreshToken),  
            sid: claims.Sid, 
            userId: claims.Uid, 
            isLongSession: request.RememberMe, 
            ct: cancellationToken
        );

        if (!tokenResult.Succeeded)
        {
            await authService.DeleteIdentityUserAsync(user);
            
            repository.Delete(user);
            await repository.SaveChangesAsync(cancellationToken);
            
            throw new BadRequestException("Could not create access.");
        }
        
        var authenticatedUserResponseDto = mapper.Map<AuthenticatedUserResponseDto>(user);
        authenticatedUserResponseDto.AccessToken = accessToken;
        authenticatedUserResponseDto.RefreshToken = refreshToken;

        return authenticatedUserResponseDto;
    }

    private async Task ThrowIfUserAlreadyExistsByUsername(string username)
    {
        if (await repository.ExistsAsync(x => x.UserName == username))
            throw new BadRequestException("Username already exists.");
    }
    private async Task ThrowIfUserAlreadyExistsByEmail(string email)
    {
        if (await repository.ExistsAsync(x => x.Email == email))
            throw new BadRequestException("Email already exists.");
    }
}