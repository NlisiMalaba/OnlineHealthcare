using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Maternal.ChildProfiles;
using HealthPlatform.Application.Vaccinations;
using HealthPlatform.Domain.Vaccinations;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Vaccinations;

public sealed class VaccinationReminderDispatcher(
    TimeProvider timeProvider,
    IVaccinationScheduleRepository scheduleRepository,
    IPatientRepository patientRepository,
    IChildProfileRepository childProfileRepository,
    IVaccinationReminderNotifier notifier,
    ILogger<VaccinationReminderDispatcher> logger) : IVaccinationReminderDispatcher
{
    public async Task<int> DispatchDueRemindersAsync(CancellationToken ct)
    {
        var asOfDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var dueEntries = await scheduleRepository.ListDueForReminderAsync(asOfDate, ct);

        if (dueEntries.Count == 0)
        {
            return 0;
        }

        var dispatched = 0;
        var sentAtUtc = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var entry in dueEntries)
        {
            ct.ThrowIfCancellationRequested();

            if (!VaccinationReminderPolicies.IsDueForReminder(entry.RecommendedDate, asOfDate))
            {
                continue;
            }

            var recipientUserId = await ResolveRecipientUserIdAsync(entry, ct);
            if (recipientUserId is null)
            {
                continue;
            }

            await notifier.NotifyVaccinationDueAsync(
                recipientUserId.Value,
                entry.ChildProfileId,
                entry.PatientId,
                entry.Id,
                entry.VaccineName,
                entry.RecommendedDate,
                entry.ChildProfileId.HasValue,
                ct);

            if (!entry.MarkReminderSent(sentAtUtc))
            {
                continue;
            }

            await scheduleRepository.UpdateAsync(entry, ct);
            dispatched++;

            logger.LogInformation(
                "Dispatched vaccination reminder for schedule entry {ScheduleEntryId}, vaccine {VaccineName}.",
                entry.Id,
                entry.VaccineName);
        }

        return dispatched;
    }

    private async Task<Guid?> ResolveRecipientUserIdAsync(VaccinationScheduleEntry entry, CancellationToken ct)
    {
        if (entry.ChildProfileId.HasValue)
        {
            var childProfile = await childProfileRepository.GetByIdAsync(entry.ChildProfileId.Value, ct);
            if (childProfile is null)
            {
                logger.LogWarning(
                    "Skipping vaccination reminder; child profile {ChildProfileId} was not found.",
                    entry.ChildProfileId);
                return null;
            }

            var guardian = await patientRepository.GetByIdAsync(childProfile.GuardianId, ct);
            if (guardian is null)
            {
                logger.LogWarning(
                    "Skipping vaccination reminder; guardian {GuardianId} was not found.",
                    childProfile.GuardianId);
                return null;
            }

            return guardian.UserId;
        }

        if (entry.PatientId.HasValue)
        {
            var patient = await patientRepository.GetByIdAsync(entry.PatientId.Value, ct);
            if (patient is null)
            {
                logger.LogWarning(
                    "Skipping vaccination reminder; patient {PatientId} was not found.",
                    entry.PatientId);
                return null;
            }

            return patient.UserId;
        }

        return null;
    }
}
