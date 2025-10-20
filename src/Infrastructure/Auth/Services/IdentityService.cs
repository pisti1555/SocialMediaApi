using Application.Common.Results;
using Application.Contracts.Services;
using Domain.Users;
using Infrastructure.Auth.Models;
using Infrastructure.Common.Exceptions;
using Infrastructure.Persistence.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Auth.Services;

public sealed class IdentityService(
    UserManager<AppIdentityUser> userManager,
    IOutsideServicesRepository<Token> tokenRepository,
    ILogger<IdentityService> logger
) : IAuthService
{
    public async Task<AppResult> SaveTokenAsync(
        string jtiHash,
        string refreshTokenHash,
        string sid,
        string userId,
        bool isLongSession, 
        CancellationToken ct = default
    )
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            logger.LogError("Token creation request failed: nameId cannot be parsed to GUID. Provided value: {UserId}", userId);
            return AppResult.Failure(["Invalid User ID format."]);
        }
        
        var token = Token.CreateToken(
            sessionId: sid,
            userId: userGuid,
            jtiHash: jtiHash,
            refreshTokenHash: refreshTokenHash,
            isLongSession: isLongSession
        );
        
        tokenRepository.Add(token);
        await tokenRepository.SaveChangesAsync(ct);
        
        return AppResult.Success();
    }

    public async Task<AppResult> UpdateTokenAsync(
        string oldRefreshTokenHash, 
        string oldJtiHash,
        string sid, 
        string uid, 
        string newRefreshTokenHash,
        string newJtiHash,
        CancellationToken ct = default
    )
    {
        var token = await tokenRepository.GetAsync(x => x.Id == sid, ct);
        if (token is null)
        {
            logger.LogWarning("Token not found by Session ID: {SessionId}", sid);
            return AppResult.Failure(["Invalid credentials."]);
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
            return AppResult.Failure(["Invalid credentials."]);
        }
        
        var result = token.Refresh(newJtiHash, newRefreshTokenHash);
        if (!result.Succeeded)
        {
            var errorMessage = result.Errors.FirstOrDefault() ?? "Unknown error.";
            
            tokenRepository.Delete(token);
            await tokenRepository.SaveChangesAsync(ct);
            
            logger.LogWarning("Token refresh failed. Token removed. Error: {ErrorMessage}", errorMessage);
            return result;
        }
        
        tokenRepository.Update(token);
        await tokenRepository.SaveChangesAsync(ct);
        logger.LogInformation("Token refreshed successfully for User: {UserId} with Session: {SessionId}.", uid, sid);
        
        return AppResult.Success();
    }

    public async Task<bool> CheckPasswordAsync(AppUser user, string password, CancellationToken ct = default)
    {
        var identityUser = await GetAppIdentityUserByAppUserId(user);
        return 
            identityUser is not null 
            && await userManager.CheckPasswordAsync(identityUser, password);
    }
    
    public async Task<AppResult> CreateIdentityUserAsync(AppUser user, string password, CancellationToken ct = default)
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