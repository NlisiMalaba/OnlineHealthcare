namespace HealthPlatform.Application.Vaccinations;

public interface IVaccinationReminderNotifier
{
    Task NotifyVaccinationDueAsync(
        Guid recipientUserId,
        Guid? childProfileId,
        Guid? patientId,
        Guid scheduleEntryId,
        string vaccineName,
        DateOnly recommendedDate,
        bool isChildProfile,
        CancellationToken ct);
}
