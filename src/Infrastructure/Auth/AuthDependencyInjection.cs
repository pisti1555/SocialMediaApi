using System.Text;
using Application.Contracts.Services;
using Infrastructure.Auth.Configuration;
using Infrastructure.Auth.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Persistence.Auth.Models;
using Persistence.DataContext;

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
        services.Configure<JwtConfiguration>(config.GetSection("Jwt"));
        services.AddSingleton<IValidateOptions<JwtConfiguration>, JwtConfigurationValidation>();
        
        // Add token and auth services
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IAuthService, IdentityService>();
        
        // Read JWT options
        var jwtOptions = config.GetSection("Jwt").Get<JwtConfiguration>();
        if (jwtOptions is null)
            throw new InvalidOperationException("JWT configuration is missing.");
        
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
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),

                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        // Add authorization
        services.AddAuthorizationBuilder()
            .AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));

        return services;
    }
}