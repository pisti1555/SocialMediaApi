using HealthChecks.UI.Client;
using Infrastructure.Persistence.Seeds;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

namespace API.Extensions;

internal static class WebApplicationExtensions
{
    internal static async Task SetupDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await DbMigrator.MigrateAsync(scope.ServiceProvider);
        await RoleDbSeeder.SeedRoles(scope.ServiceProvider);
        await AdminDbSeeder.SeedAsync(scope.ServiceProvider);
    }
    
    internal static void SetupScalar(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference("/scalar", (options, context) =>
        {
            options.Servers = [];
    
            options.Title = "Social Media API";
            options.BaseServerUrl = $"{context.Request.Scheme}://{context.Request.Host.Value}";
        });
    }
    
    internal static void SetupHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        }).RequireAuthorization(opt => opt.RequireRole("Admin"));
    }
}