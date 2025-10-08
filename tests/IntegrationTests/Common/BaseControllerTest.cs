using Application.Contracts.Services;
using Domain.Users;
using Infrastructure.Auth.Models;
using Infrastructure.Persistence.DataContext;
using IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests.Common;

public abstract class BaseControllerTest : IClassFixture<CustomWebApplicationFactoryFixture>
{
    private readonly CustomWebApplicationFactoryFixture _factory;
    
    private readonly ITokenService _tokenService;
    private readonly RoleManager<AppIdentityRole> _roleManager;
    
    protected readonly HttpClient Client;
    protected readonly AppDbContext DbContext;
    protected readonly AppIdentityDbContext IdentityDbContext;
    protected readonly UserManager<AppIdentityUser> UserManager;
    protected readonly ICacheService Cache;

    protected BaseControllerTest(CustomWebApplicationFactoryFixture factory)
    {
        _factory = factory;
        
        var scope = factory.Services.CreateScope();
        
        _tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        _roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppIdentityRole>>();
        
        Client = factory.CreateClient();
        DbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IdentityDbContext = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
        UserManager = scope.ServiceProvider.GetRequiredService<UserManager<AppIdentityUser>>();
        Cache = factory.Services.GetRequiredService<ICacheService>();
    }

    protected async Task<HttpClient> GetAuthenticatedClientAsync(AppUser user)
    {
        await CreateRoles();
        
        var identityUser = await IdentityDbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        if (identityUser is null)
        {
            identityUser = new AppIdentityUser
            {
                Id = user.Id,
                UserName = user.UserName.ToLower(),
                Email = user.Email.ToLower(),
                SecurityStamp = Guid.NewGuid().ToString()
            };
            
            var createResult = await UserManager.CreateAsync(identityUser, "Test-Password-123");
            if (!createResult.Succeeded)
            {
                throw new Exception("Failed to create identity user.");
            }
            
            var addRoleResult = await UserManager.AddToRoleAsync(identityUser, "User");
            if (!addRoleResult.Succeeded)
            {
                throw new Exception("Failed to add user to role.");
            }
        }
        
        var token = _tokenService.CreateAccessToken(
            uid: user.Id.ToString(), 
            name: user.UserName, 
            email: user.Email,
            roles: ["User"],
            sid: null
        );
        
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return client;
    }
    
    protected async Task CreateRoles()
    {
        List<AppIdentityRole> roles = [
            new("Admin"), 
            new("User")
        ];
    
        foreach (var r in roles)
        {
            if (r.Name is not null && await _roleManager.RoleExistsAsync(r.Name)) continue;
            await _roleManager.CreateAsync(r);
        }
    }
}