using Infrastructure.Auth.Models;
using Infrastructure.Common.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence.Seeds;

public static class RoleDbSeeder
{
    public static async Task SeedRoles(IServiceProvider sp)
    {
        var roleManager = sp.GetRequiredService<RoleManager<AppIdentityRole>>();
        
        var roles = new [] { "Admin", "User" };
    
        foreach (var role in roles)
        {
            if (await roleManager.RoleExistsAsync(role)) continue;
    
            var result = await roleManager.CreateAsync(new AppIdentityRole(role));
            if (!result.Succeeded)
            {
                throw new PersistenceException($"Failed to create role {role} on application startup.");
            }
        }
    }
}