using Application;
using Application.Contracts.Services;
using Domain.Users;
using Infrastructure.Auth.Exceptions;
using Microsoft.AspNetCore.Identity;
using Persistence.Auth.Models;

namespace Infrastructure.Auth.Services;

public sealed class IdentityService(UserManager<AppIdentityUser> userManager) : IAuthService
{
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