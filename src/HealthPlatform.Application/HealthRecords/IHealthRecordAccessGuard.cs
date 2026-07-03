namespace HealthPlatform.Application.HealthRecords;

public interface IHealthRecordAccessGuard
{
    Task EnsureDoctorCanReadAsync(Guid healthRecordId, Guid doctorId, CancellationToken ct);
}
