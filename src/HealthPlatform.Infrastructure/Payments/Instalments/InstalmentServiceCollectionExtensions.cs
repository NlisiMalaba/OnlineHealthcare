using HealthPlatform.Application.Payments.Instalments;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HealthPlatform.Infrastructure.Payments.Instalments;

public static class InstalmentServiceCollectionExtensions
{
    public static IServiceCollection AddInstalmentServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<InstalmentPlanOptions>(
            configuration.GetSection(InstalmentPlanOptions.SectionName));

        services.AddScoped<IInstalmentPlanRepository, Persistence.Repositories.InstalmentPlanRepository>();
        services.AddScoped<IInstalmentPaymentRepository, Persistence.Repositories.InstalmentPaymentRepository>();
        services.AddScoped<IInstalmentDueReminderDispatcher, InstalmentDueReminderDispatcher>();
        services.AddScoped<IInstalmentMissedPaymentProcessor, InstalmentMissedPaymentProcessor>();
        services.AddSingleton<IInstalmentDueReminderNotifier, LoggingInstalmentDueReminderNotifier>();
        services.AddSingleton<IInstalmentMissedPaymentNotifier, LoggingInstalmentMissedPaymentNotifier>();

        return services;
    }
}
