using HealthPlatform.Application.Payments;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Payments;

public sealed class LoggingPaymentFailedNotifier(ILogger<LoggingPaymentFailedNotifier> logger)
    : IPaymentFailedNotifier
{
    public Task NotifyPaymentFailedAsync(
        Guid patientUserId,
        Guid patientId,
        Guid paymentId,
        Guid? appointmentId,
        Guid? medicationOrderId,
        string failureCode,
        string failureMessage,
        DateTime retentionExpiresAtUtc,
        CancellationToken ct)
    {
        logger.LogWarning(
            "Payment failed for patient {PatientId} payment {PaymentId}: {FailureCode} {FailureMessage}. Retry until {RetentionExpiresAtUtc}.",
            patientId,
            paymentId,
            failureCode,
            failureMessage,
            retentionExpiresAtUtc);

        return Task.CompletedTask;
    }
}
