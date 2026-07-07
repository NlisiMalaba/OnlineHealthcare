namespace HealthPlatform.Application.HealthRecords;

public interface IHealthRecordAccessGuard
{
    Task EnsureDoctorCanReadAsync(
        Guid healthRecordId,
        Guid doctorId,
        string accessOperation,
        CancellationToken ct);
}
