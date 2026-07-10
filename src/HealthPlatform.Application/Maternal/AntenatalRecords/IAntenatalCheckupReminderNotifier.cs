namespace HealthPlatform.Application.Maternal.AntenatalRecords;

public interface IAntenatalCheckupReminderNotifier
{
    Task NotifyAntenatalCheckupReminderAsync(
        Guid patientUserId,
        Guid obstetricDoctorUserId,
        Guid antenatalRecordId,
        DateOnly estimatedDueDate,
        bool highFrequency,
        CancellationToken ct);
}
