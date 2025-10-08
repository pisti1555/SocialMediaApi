using Application.Common.Results;
using Application.Contracts.Auth;
using Application.Contracts.Services;
using Domain.Common.Exceptions.CustomExceptions;
using Domain.Users;
using Infrastructure.Auth.Exceptions;
using Infrastructure.Auth.Models;
using Infrastructure.Persistence.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Auth.Services;

public sealed class IdentityService(
    UserManager<AppIdentityUser> userManager, 
    ITokenService tokenService,
    IOutsideServicesRepository<Token> tokenRepository,
    IHasher hasher
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
        var jti = claims.FirstOrDefault(x => x.Type == TokenClaims.TokenId)?.Value ?? throw new IdentityOperationException("JTI claim not found.");
        var sid = claims.FirstOrDefault(x => x.Type == TokenClaims.SessionId)?.Value ?? throw new IdentityOperationException("SID claim not found.");
        var nameId = claims.FirstOrDefault(x => x.Type == TokenClaims.UserId)?.Value ?? throw new IdentityOperationException("User ID claim not found.");
        
        var userId = Guid.TryParse(nameId, out var guid) ? guid : throw new IdentityOperationException("Invalid user ID.");
        
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
            throw new UnauthorizedException("Invalid request.");
        }
        
        var userId = Guid.TryParse(uid, out var guid) ? guid : throw new UnauthorizedException("Invalid user ID.");
        var oldJtiHash = hasher.CreateHash(oldJti);
        var oldRefreshTokenHash = hasher.CreateHash(oldRefreshToken);
        
        var token = await tokenRepository.GetAsync(x => 
            x.Id == sid
            && x.UserId == userId
            && x.JtiHash == oldJtiHash
            && x.RefreshTokenHash == oldRefreshTokenHash,
            ct
        );
        if (token is null || token.IsExpired())
        {
            throw new UnauthorizedException("Invalid request.");
        }
        
        token.Refresh(hasher.CreateHash(newJti), hasher.CreateHash(newRefreshToken));
        
        tokenRepository.Update(token);
        await tokenRepository.SaveChangesAsync(ct);
    }

    public async Task<bool> CheckPasswordAsync(AppUser user, string password, CancellationToken ct = default)
    {
        var identityUser = await GetAppIdentityUserByAppUserId(user);
        return 
            identityUser is not null 
            && await userManager.CheckPasswordAsync(identityUser, password);
    }
    
    public async Task<IdentityUserCreationResult> CreateIdentityUserFromAppUserAsync(AppUser user, string password, CancellationToken ct = default)
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
        
        var succeeded = creationResult.Succeeded && roleResult.Succeeded;
        var errors = new List<string>();
        errors.AddRange(creationResult.Errors.Select(x => x.Description));
        errors.AddRange(roleResult.Errors.Select(x => x.Description));
        
        return new IdentityUserCreationResult
        {
            Succeeded = succeeded,
            Errors = errors
        };
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