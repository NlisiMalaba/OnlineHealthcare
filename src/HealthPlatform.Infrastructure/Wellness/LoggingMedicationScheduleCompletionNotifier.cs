using HealthPlatform.Application.Wellness;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Wellness;

public sealed class LoggingMedicationScheduleCompletionNotifier(
    ILogger<LoggingMedicationScheduleCompletionNotifier> logger) : IMedicationScheduleCompletionNotifier
{
    public Task NotifyScheduleCompletedAsync(MedicationScheduleCompletionNotice notice, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Medication schedule {ScheduleId} completed. Notifying patient {PatientUserId} and doctor {DoctorUserId}.",
            notice.ScheduleId,
            notice.PatientUserId,
            notice.DoctorUserId);
        return Task.CompletedTask;
    }
}
