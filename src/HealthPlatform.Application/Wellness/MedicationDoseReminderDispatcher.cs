using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Wellness;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Wellness;

public sealed class MedicationDoseReminderDispatcher(
    TimeProvider timeProvider,
    IMedicationDoseReminderRepository reminderRepository,
    IPatientRepository patientRepository,
    IMedicationDoseReminderNotifier notifier,
    ILogger<MedicationDoseReminderDispatcher> logger) : IMedicationDoseReminderDispatcher
{
    public async Task<int> DispatchDueRemindersAsync(CancellationToken ct)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var dueDoses = await reminderRepository.ListDueDosesAsync(
            nowUtc,
            WellnessPolicies.DoseReminderLookbackWindow,
            WellnessPolicies.DoseReminderBatchSize,
            ct);

        if (dueDoses.Count == 0)
        {
            return 0;
        }

        var dispatched = 0;
        foreach (var dueDose in dueDoses)
        {
            ct.ThrowIfCancellationRequested();

            var patient = await patientRepository.GetByIdAsync(dueDose.PatientId, ct);
            if (patient is null)
            {
                logger.LogWarning(
                    "Skipping medication dose reminder for schedule {ScheduleId}; patient {PatientId} was not found.",
                    dueDose.ScheduleId,
                    dueDose.PatientId);
                continue;
            }

            await notifier.NotifyDoseReminderAsync(
                patient.UserId,
                dueDose.ScheduleId,
                dueDose.ScheduledAtUtc,
                ct);

            var reminder = MedicationDoseReminder.RecordSent(
                dueDose.ScheduleId,
                dueDose.PatientId,
                dueDose.ScheduledAtUtc,
                nowUtc);

            await reminderRepository.AddSentReminderAsync(reminder, ct);
            dispatched++;

            logger.LogInformation(
                "Dispatched medication dose reminder for schedule {ScheduleId} at {ScheduledAtUtc}.",
                dueDose.ScheduleId,
                dueDose.ScheduledAtUtc);
        }

        if (dispatched > 0)
        {
            await reminderRepository.SaveChangesAsync(ct);
        }

        return dispatched;
    }
}
