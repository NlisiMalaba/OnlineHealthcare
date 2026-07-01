namespace HealthPlatform.Application.Wellness;

public interface IMedicationDoseReminderNotifier
{
  Task NotifyDoseReminderAsync(
      Guid patientUserId,
      Guid scheduleId,
      DateTime scheduledAtUtc,
      CancellationToken ct);
}
