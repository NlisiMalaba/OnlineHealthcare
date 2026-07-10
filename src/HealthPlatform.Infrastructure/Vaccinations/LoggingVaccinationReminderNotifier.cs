using HealthPlatform.Application.Vaccinations;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Vaccinations;

public sealed class LoggingVaccinationReminderNotifier(ILogger<LoggingVaccinationReminderNotifier> logger)
    : IVaccinationReminderNotifier
{
    public Task NotifyVaccinationDueAsync(
        Guid recipientUserId,
        Guid? childProfileId,
        Guid? patientId,
        Guid scheduleEntryId,
        string vaccineName,
        DateOnly recommendedDate,
        bool isChildProfile,
        CancellationToken ct)
    {
        logger.LogInformation(
            "Vaccination reminder for user {RecipientUserId}, child profile {ChildProfileId}, patient {PatientId}, entry {ScheduleEntryId}, vaccine {VaccineName}, due {RecommendedDate}, is child {IsChildProfile}.",
            recipientUserId,
            childProfileId,
            patientId,
            scheduleEntryId,
            vaccineName,
            recommendedDate,
            isChildProfile);

        return Task.CompletedTask;
    }
}
