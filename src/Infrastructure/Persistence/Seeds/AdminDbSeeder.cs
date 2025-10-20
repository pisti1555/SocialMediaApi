using Domain.Users;
using Domain.Users.Factories;
using Infrastructure.Auth.Models;
using Infrastructure.Common.Exceptions;
using Infrastructure.Persistence.DataContext.AppDb;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence.Seeds;

public static class AdminDbSeeder
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        var admin = CreateEntities();
        await SaveAdmin(sp, admin.appAdminUser, admin.identityAdminUser);
    }

    private static (AppUser appAdminUser, AppIdentityUser identityAdminUser) CreateEntities()
    {
        var appAdminUser = AppUserFactory.Create("admin", "admin@admin.com", "Admin", "Admin", DateOnly.Parse("2000-01-01"), true);
        var identityAdminUser = new AppIdentityUser
        {
            Id = appAdminUser.Id,
            UserName = appAdminUser.UserName,
            Email = appAdminUser.Email,
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        
        return (appAdminUser, identityAdminUser);
    }

    private static async Task SaveAdmin(IServiceProvider sp, AppUser appAdminUser, AppIdentityUser identityAdminUser)
    {
        var appDbContext = sp.GetRequiredService<AppDbContext>();
        var userManager = sp.GetRequiredService<UserManager<AppIdentityUser>>();
        
        var existingAdminUser = await appDbContext.Users.FirstOrDefaultAsync(x => x.UserName == appAdminUser.UserName);
        if (existingAdminUser is null)
        {
            appDbContext.Users.Add(appAdminUser);
            if (await appDbContext.SaveChangesAsync() == 0)
            {
                throw new PersistenceException("Failed to save admin user to AppDb on application startup.");
            }
        }
        
        var existingAdminIdentityUser = await userManager.FindByNameAsync(identityAdminUser.UserName ?? string.Empty);
        if (existingAdminIdentityUser is null)
        {
            var creationResult = await userManager.CreateAsync(identityAdminUser, "Admin-123");
            var addToAdminRoleResult = await userManager.AddToRoleAsync(identityAdminUser, "Admin");
            var addToUserRoleResult = await userManager.AddToRoleAsync(identityAdminUser, "User");

            if (!creationResult.Succeeded || !addToAdminRoleResult.Succeeded || !addToUserRoleResult.Succeeded)
            {
                await userManager.DeleteAsync(identityAdminUser);
                
                appDbContext.Users.Remove(appAdminUser);
                await appDbContext.SaveChangesAsync();
                throw new PersistenceException("Failed to save admin user to IdentityDb on application startup.");
            }
        }
    }
}