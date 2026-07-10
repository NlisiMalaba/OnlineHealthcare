using HealthPlatform.Application.Maternal.BirthPlans;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Maternal;

public sealed class LoggingBirthPlanUpdatedNotifier(ILogger<LoggingBirthPlanUpdatedNotifier> logger)
    : IBirthPlanUpdatedNotifier
{
    public Task NotifyBirthPlanUpdatedAsync(
        Guid obstetricDoctorUserId,
        Guid birthPlanId,
        Guid antenatalRecordId,
        Guid patientId,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Birth plan updated notification for birth plan {BirthPlanId}, antenatal record {AntenatalRecordId}, patient {PatientId}, obstetric doctor user {ObstetricDoctorUserId}.",
            birthPlanId,
            antenatalRecordId,
            patientId,
            obstetricDoctorUserId);
        return Task.CompletedTask;
    }
}
