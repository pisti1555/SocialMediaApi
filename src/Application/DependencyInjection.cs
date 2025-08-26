using Application.Common.Behaviors;
using Cortex.Mediator.DependencyInjection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddCortexMediator(
            configuration,
            [typeof(DependencyInjection)],
            options =>
            {
                options.AddOpenCommandPipelineBehavior(typeof(ValidationBehavior<,>));
                options.AddOpenCommandPipelineBehavior(typeof(TransactionBehavior<,>));
            }
        );

        return services;
    }
}