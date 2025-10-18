using System.Text;
using Application.Contracts.Services;
using Infrastructure.Auth.Configuration;
using Infrastructure.Auth.Models;
using Infrastructure.Auth.Services;
using Infrastructure.Persistence.DataContext.AppIdentityDb;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Auth;

internal static class AuthDependencyInjection
{
    internal static IServiceCollection SetupAuth(this IServiceCollection services, IConfiguration config)
    {
        // Set up identity services
        services
            .AddIdentityCore<AppIdentityUser>(options => 
            {
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true; 
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<AppIdentityRole>()
            .AddRoleManager<RoleManager<AppIdentityRole>>()
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();
        
        // Validate JWT options
        services.AddOptions<JwtConfiguration>()
            .BindConfiguration("Jwt")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        // Add token and auth services
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IAuthService, IdentityService>();
        
        // Read JWT options
        var serviceProvider = services.BuildServiceProvider();
        var jwtConfiguration = serviceProvider.GetRequiredService<IOptions<JwtConfiguration>>().Value;
        
        // Add authentication
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration.SecretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtConfiguration.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtConfiguration.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        // Add authorization
        services.AddAuthorizationBuilder()
            .AddPolicy("Admin", policy => policy.RequireRole("Admin"));

        return services;
    }
}