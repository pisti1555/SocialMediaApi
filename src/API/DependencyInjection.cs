namespace API;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder => 
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
            );
        });
        
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        
        return services;
    }
}