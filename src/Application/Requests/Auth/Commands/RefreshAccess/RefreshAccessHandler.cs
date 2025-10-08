using Application.Contracts.Auth;
using Application.Contracts.Services;
using Application.Responses;
using Cortex.Mediator.Commands;
using Domain.Common.Exceptions.CustomExceptions;

namespace Application.Requests.Auth.Commands.RefreshAccess;

public class RefreshAccessHandler(
    IAuthService authService,
    ITokenService tokenService
) : ICommandHandler<RefreshAccessCommand, TokenResponseDto>
{
    public async Task<TokenResponseDto> Handle(RefreshAccessCommand command, CancellationToken cancellationToken)
    {
        var claims = tokenService.GetClaimsFromToken(command.AccessToken);
        var isValid = tokenService.ValidateToken(token: command.AccessToken, claims: claims, withExpiration: false);
        if (!isValid)
        {
            throw new UnauthorizedException("Invalid access token.");
        }
        
        var roles = claims.FindAll(x => x.Type == TokenClaims.Role).Select(x => x.Value).ToList();
        var uid = claims.FirstOrDefault(x => x.Type == TokenClaims.UserId)?.Value;
        var name = claims.FirstOrDefault(x => x.Type == TokenClaims.Name)?.Value;
        var email = claims.FirstOrDefault(x => x.Type == TokenClaims.Email)?.Value;
        var sid = claims.FirstOrDefault(x => x.Type == TokenClaims.SessionId)?.Value;
        var jti = claims.FirstOrDefault(x => x.Type == TokenClaims.TokenId)?.Value;
        
        var newAccessToken = tokenService.CreateAccessToken(uid, name, email, roles, sid);
        var newRefreshToken = tokenService.CreateRefreshToken();
        
        var newClaims = tokenService.GetClaimsFromToken(newAccessToken);
        var newJti = newClaims.FirstOrDefault(x => x.Type == TokenClaims.TokenId)?.Value;
        
        await authService.UpdateTokenAsync(
            oldRefreshToken: command.RefreshToken,
            newRefreshToken: newRefreshToken,
            sid: sid,
            uid: uid,
            oldJti: jti,
            newJti: newJti,
            cancellationToken
        );
        
        return new TokenResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }
}