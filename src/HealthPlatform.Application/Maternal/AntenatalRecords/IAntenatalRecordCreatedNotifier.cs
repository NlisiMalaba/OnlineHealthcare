namespace HealthPlatform.Application.Maternal.AntenatalRecords;

public interface IAntenatalRecordCreatedNotifier
{
    Task NotifyAntenatalRecordCreatedAsync(
        Guid patientUserId,
        Guid obstetricDoctorUserId,
        Guid antenatalRecordId,
        DateOnly estimatedDueDate,
        int recommendedCheckupCount,
        CancellationToken ct);
}
