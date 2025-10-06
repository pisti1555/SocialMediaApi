using API;
using API.Middlewares;
using Application;
using Infrastructure;
using Infrastructure.Auth.Exceptions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Auth.Models;
using Persistence.DataContext;
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
builder.Services.AddPersistence(builder.Configuration);
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

// Migrate database and create roles
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var identityDb = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppIdentityRole>>();
    
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
}

app.Run();

// Expose Program class for integration testing
public partial class Program { }