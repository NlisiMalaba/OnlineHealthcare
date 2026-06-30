using HealthPlatform.Application.Payments.CreditLine;
using Microsoft.Extensions.DependencyInjection;

namespace HealthPlatform.Infrastructure.Payments.CreditLine;

public static class CreditLineServiceCollectionExtensions
{
    public static IServiceCollection AddCreditLineServices(this IServiceCollection services)
    {
        services.AddScoped<IPatientCreditLineRepository, Persistence.Repositories.PatientCreditLineRepository>();
        services.AddScoped<ICreditLineTransactionRepository, Persistence.Repositories.CreditLineTransactionRepository>();
        services.AddScoped<ICreditRepaymentReminderDispatcher, CreditRepaymentReminderDispatcher>();
        services.AddSingleton<ICreditRepaymentReminderNotifier, LoggingCreditRepaymentReminderNotifier>();
        services.AddSingleton<ICreditBalanceWarningNotifier, LoggingCreditBalanceWarningNotifier>();
        return services;
    }
}
