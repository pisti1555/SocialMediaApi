using Application.Contracts.Services;
using Application.Responses;
using Cortex.Mediator.Commands;
using Domain.Common.Exceptions.CustomExceptions;

namespace Application.Requests.Auth.Commands.RefreshAccess;

public class RefreshAccessHandler(
    IAuthService authService,
    ITokenService tokenService,
    IHasher hasher
) : ICommandHandler<RefreshAccessCommand, TokenResponseDto>
{
    public async Task<TokenResponseDto> Handle(RefreshAccessCommand command, CancellationToken cancellationToken)
    {
        var claimsResult = tokenService.GetValidatedClaimsFromToken(command.AccessToken);
        if (!claimsResult.Succeeded)
        {
            throw new UnauthorizedException("Invalid access token.");
        }
        
        var isValid = tokenService.ValidateToken(token: command.AccessToken, withExpiration: false);
        if (!isValid)
        {
            throw new UnauthorizedException("Invalid access token.");
        }
        
        var claims = claimsResult.Data;
        
        var newAccessToken = tokenService.CreateAccessToken(claims.Uid, claims.Name, claims.Email, claims.Roles, claims.Sid);
        var newRefreshToken = tokenService.CreateRefreshToken();
        
        var newClaims = tokenService.GetValidatedClaimsFromToken(newAccessToken).Data;

        var result = await authService.UpdateTokenAsync(
            oldRefreshTokenHash: hasher.CreateHash(command.RefreshToken),
            oldJtiHash: hasher.CreateHash(claims.Jti),
            sid: newClaims.Sid,
            uid: newClaims.Uid,
            newRefreshTokenHash: hasher.CreateHash(newRefreshToken),
            newJtiHash: hasher.CreateHash(newClaims.Jti),
            cancellationToken
        );

        if (!result.Succeeded)
        {
            throw new UnauthorizedException("Could not refresh access token.");
        }
        
        return new TokenResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }
}