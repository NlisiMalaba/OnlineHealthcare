using HealthPlatform.Application.Payments.CreditLine;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Payments.CreditLine;

public sealed class LoggingCreditBalanceWarningNotifier(
    ILogger<LoggingCreditBalanceWarningNotifier> logger) : ICreditBalanceWarningNotifier
{
    public Task NotifyBalanceWarningAsync(
        Guid patientUserId,
        Guid patientId,
        long outstandingBalanceMinorUnits,
        long creditLimitMinorUnits,
        string currency,
        CancellationToken ct)
    {
        logger.LogWarning(
            "Credit balance warning for patient {PatientId}: outstanding {Outstanding} of limit {Limit} {Currency}.",
            patientId,
            outstandingBalanceMinorUnits,
            creditLimitMinorUnits,
            currency);

        return Task.CompletedTask;
    }
}
