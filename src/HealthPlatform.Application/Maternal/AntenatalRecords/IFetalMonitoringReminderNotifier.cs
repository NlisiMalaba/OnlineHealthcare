namespace HealthPlatform.Application.Maternal.AntenatalRecords;

public interface IFetalMonitoringReminderNotifier
{
    Task NotifyFetalMonitoringReminderAsync(
        Guid patientUserId,
        Guid antenatalRecordId,
        int intervalDays,
        CancellationToken ct);
}
