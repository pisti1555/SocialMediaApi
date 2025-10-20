using API;
using API.Extensions;
using API.Middlewares;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
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

app.UseSerilogRequestLogging();
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseAuthentication();
app.UseAuthorization();
app.UseCors();
app.UseStaticFiles();
app.MapControllers();
app.SetupHealthChecks();
app.SetupScalar();
await app.SetupDatabaseAsync();

app.Run();

// Expose Program class for integration testing
public partial class Program { }