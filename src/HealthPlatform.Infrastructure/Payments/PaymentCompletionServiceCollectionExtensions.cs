using HealthPlatform.Application.Payments;
using Microsoft.Extensions.DependencyInjection;

namespace HealthPlatform.Infrastructure.Payments;

public static class PaymentCompletionServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentCompletionServices(this IServiceCollection services)
    {
        services.AddScoped<IPaymentRepository, Persistence.Repositories.PaymentRepository>();
        services.AddSingleton<IPaymentReceiptGenerator, TextPaymentReceiptGenerator>();
        services.AddScoped<IPaymentCompletionService, PaymentCompletionService>();
        services.AddScoped<IPaymentFailureService, PaymentFailureService>();
        services.AddSingleton<IPaymentFailedNotifier, LoggingPaymentFailedNotifier>();
        return services;
    }
}
