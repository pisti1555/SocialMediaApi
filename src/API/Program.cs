using API;
using API.Middlewares;
using Application;
using Domain.Users.Factories;
using HealthChecks.UI.Client;
using Infrastructure;
using Infrastructure.Auth.Exceptions;
using Infrastructure.Auth.Models;
using Infrastructure.Persistence.DataContext.AppDb;
using Infrastructure.Persistence.DataContext.AppIdentityDb;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Use Serilog
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
    config.Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);
});

// Add layers to the program
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApi();


var app = builder.Build();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors();

app.UseStaticFiles();

app.MapControllers();
app.MapOpenApi();
app.MapScalarApiReference("/scalar", (options, context) =>
{
    options.Servers = [];
    
    options.Title = "Social Media API";
    options.BaseServerUrl = $"{context.Request.Scheme}://{context.Request.Host.Value}";
});
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).RequireAuthorization(opt =>
{
    opt.RequireRole("Admin");
});

// Migrate database and create roles
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var identityDb = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppIdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppIdentityUser>>();
    
    db.Database.Migrate();
    logger.LogInformation("Migrated database successfully.");
    identityDb.Database.Migrate();
    logger.LogInformation("Migrated identity database successfully.");

    List<AppIdentityRole> roles = [
        new("Admin"), 
        new("User")
    ];
    
    foreach (var r in roles)
    {
        if (r.Name is not null && await roleManager.RoleExistsAsync(r.Name))
        {
            logger.LogInformation("Role {RoleName} already exists, skipping...", r.Name);
            continue;
        }
        
        var result = await roleManager.CreateAsync(r);
        if (!result.Succeeded)
        {
            throw new IdentityOperationException("Failed to create roles on application startup.");
        }
        logger.LogInformation("Role {RoleName} created successfully.", r.Name);
    }
    
    var existingAdminAppDb = await db.Users.FirstOrDefaultAsync(u => u.UserName == "admin");
    if (existingAdminAppDb is null)
    {
        var adminAppDb = AppUserFactory.Create(
            "admin", "admin@admin.com", "Admin", "Admin", DateOnly.Parse("2000-01-01"), true
        );
        db.Users.Add(adminAppDb);
        await db.SaveChangesAsync();
        logger.LogInformation("Admin user created in AppDb.");
    }
    else
    {
        logger.LogInformation("Admin user already exists in AppDb, skipped.");
    }
    
    var existingAdminIdentityDb = await userManager.FindByNameAsync("admin");
    if (existingAdminIdentityDb is null)
    {
        var adminIdentityDb = new AppIdentityUser
        {
            Id = existingAdminAppDb?.Id ?? Guid.NewGuid(),
            UserName = "admin",
            Email = "admin@admin.com",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        await userManager.CreateAsync(adminIdentityDb, "Admin-123");
        await userManager.AddToRoleAsync(adminIdentityDb, "Admin");
        await userManager.AddToRoleAsync(adminIdentityDb, "User");
        logger.LogInformation("Admin user created in IdentityDb.");
    }
    else
    {
        logger.LogInformation("Admin user already exists in IdentityDb, skipped.");
    }
}

app.Run();

// Expose Program class for integration testing
public partial class Program { }