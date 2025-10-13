using Application.Common.Results;
using Application.Contracts.Auth;
using Application.Contracts.Services;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;
using Infrastructure.Auth.Exceptions;
using Infrastructure.Auth.Models;
using Infrastructure.Persistence.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Auth.Services;

public sealed class IdentityService(
    UserManager<AppIdentityUser> userManager, 
    ITokenService tokenService,
    IOutsideServicesRepository<Token> tokenRepository,
    IHasher hasher,
    ILogger<IdentityService> logger
) : IAuthService
{
    public async Task SaveTokenAsync(
        string accessToken,
        string refreshToken,
        bool isLongSession, 
        CancellationToken ct = default
    )
    {
        var claims = tokenService.GetClaimsFromToken(accessToken);
        var jti = claims.FirstOrDefault(x => x.Type == TokenClaims.TokenId)?.Value ?? throw new UnauthorizedException("Jwt ID claim not found.");
        var sid = claims.FirstOrDefault(x => x.Type == TokenClaims.SessionId)?.Value ?? throw new UnauthorizedException("Session ID claim not found.");
        var nameId = claims.FirstOrDefault(x => x.Type == TokenClaims.UserId)?.Value ?? throw new UnauthorizedException("User ID claim not found.");

        if (!Guid.TryParse(nameId, out var userId))
        {
            logger.LogError("Token creation request failed: nameId cannot be parsed to GUID. Provided value: {UserId}", nameId);
            throw new UnauthorizedException("Invalid User ID format.");
        }
        
        var token = Token.CreateToken(
            sessionId: sid,
            userId: userId,
            jtiHash: hasher.CreateHash(jti),
            refreshTokenHash: hasher.CreateHash(refreshToken),
            isLongSession: isLongSession
        );
        
        tokenRepository.Add(token);
        await tokenRepository.SaveChangesAsync(ct);
    }

    public async Task UpdateTokenAsync(
        string? oldRefreshToken, 
        string newRefreshToken,
        string? sid, 
        string? uid, 
        string? oldJti,
        string? newJti,
        CancellationToken ct = default
    )
    {
        if (
            string.IsNullOrWhiteSpace(oldRefreshToken) 
            || string.IsNullOrWhiteSpace(sid) 
            || string.IsNullOrWhiteSpace(uid) 
            || string.IsNullOrWhiteSpace(oldJti)
            || string.IsNullOrWhiteSpace(newJti)
        )
        {
            throw new UnauthorizedException("Missing claims.");
        }
        
        var oldJtiHash = hasher.CreateHash(oldJti);
        var oldRefreshTokenHash = hasher.CreateHash(oldRefreshToken);

        var token = await tokenRepository.GetAsync(x => x.Id == sid, ct);
        if (token is null)
        {
            logger.LogWarning("Token not found by Session ID: {SessionId}", sid);
            throw new UnauthorizedException("Token cannot be found.");
        }

        if (token.UserId.ToString() != uid || token.JtiHash != oldJtiHash || token.RefreshTokenHash != oldRefreshTokenHash)
        {
            logger.LogWarning(
                "Invalid token refresh attempt. Token has been removed. " +
                "Provided claims -> UserId: {ProvidedUserId}, JtiHash: {ProvidedJtiHash}, RefreshTokenHash: {ProvidedRefreshTokenHash} " +
                "Stored token -> UserId: {TokenUserId}, JtiHash: {TokenJtiHash}, RefreshTokenHash: {TokenRefreshTokenHash}.", 
                uid, oldJtiHash, oldRefreshTokenHash, token.UserId, token.JtiHash, token.RefreshTokenHash
            );
            
            tokenRepository.Delete(token);
            await tokenRepository.SaveChangesAsync(ct);
            throw new UnauthorizedException("Invalid credentials. Token has been revoked.");
        }
        
        var result = token.Refresh(hasher.CreateHash(newJti), hasher.CreateHash(newRefreshToken));
        if (!result.Succeeded)
        {
            var errorMessage = result.Errors.FirstOrDefault() ?? "Unknown error.";
            
            tokenRepository.Delete(token);
            await tokenRepository.SaveChangesAsync(ct);
            
            logger.LogWarning("Token refresh failed. Token removed. Error: {ErrorMessage}", errorMessage);
            throw new UnauthorizedException($"{errorMessage} Token has been revoked.");
        }
        
        tokenRepository.Update(token);
        await tokenRepository.SaveChangesAsync(ct);
        logger.LogInformation("Token refreshed successfully for User: {UserId} with Session: {SessionId}.", uid, sid);
    }

    public async Task<bool> CheckPasswordAsync(AppUser user, string password, CancellationToken ct = default)
    {
        var identityUser = await GetAppIdentityUserByAppUserId(user);
        return 
            identityUser is not null 
            && await userManager.CheckPasswordAsync(identityUser, password);
    }
    
    public async Task<AppResult> CreateIdentityUserFromAppUserAsync(AppUser user, string password, CancellationToken ct = default)
    {
        var identityUser = new AppIdentityUser
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var creationResult = await userManager.CreateAsync(identityUser, password);
        var roleResult = await userManager.AddToRoleAsync(identityUser, "User");
        
        var errors = new List<string>();
        errors.AddRange(creationResult.Errors.Select(x => x.Description));
        errors.AddRange(roleResult.Errors.Select(x => x.Description));

        return errors.Count > 0 ? AppResult.Failure(errors) : AppResult.Success();
    }

    public async Task DeleteIdentityUserAsync(AppUser user)
    {
        var identityUser = await GetAppIdentityUserByAppUserId(user);
        if (identityUser is null)
        {
            throw new IdentityOperationException("IdentityUser not found by AppUser.");
        }
        
        var roles = await userManager.GetRolesAsync(identityUser);
        
        var removeRolesResult = await userManager.RemoveFromRolesAsync(identityUser, roles);
        if (!removeRolesResult.Succeeded)
        {
            throw new IdentityOperationException($"Failed to remove roles: {string.Join(", ", removeRolesResult.Errors.Select(e => e.Description))}");
        }
        
        var deleteResult = await userManager.DeleteAsync(identityUser);
        if (!deleteResult.Succeeded)
        {
            throw new IdentityOperationException($"Failed to delete user: {string.Join(", ", deleteResult.Errors.Select(e => e.Description))}");
        }
    }

    public async Task<IEnumerable<string>> GetRolesAsync(AppUser user, CancellationToken ct = default)
    {
        var identityUser = await GetAppIdentityUserByAppUserId(user);
        return identityUser != null
            ? await userManager.GetRolesAsync(identityUser)
            : new List<string>();
    }
    
    private async Task<AppIdentityUser?> GetAppIdentityUserByAppUserId(AppUser user)
    {
        return await userManager.FindByIdAsync(user.Id.ToString());
    }
}