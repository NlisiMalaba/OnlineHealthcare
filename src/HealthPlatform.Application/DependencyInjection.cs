using System.Reflection;
using FluentValidation;
using HealthPlatform.Application.Behaviors;
using HealthPlatform.Application.Outbox;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace HealthPlatform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();
        return services;
    }
}
