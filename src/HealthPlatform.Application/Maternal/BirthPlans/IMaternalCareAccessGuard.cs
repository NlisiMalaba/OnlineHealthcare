namespace HealthPlatform.Application.Maternal.BirthPlans;

public interface IMaternalCareAccessGuard
{
    Task EnsureCanReadBirthPlanAsync(
        Guid antenatalRecordId,
        Guid patientId,
        Guid obstetricDoctorId,
        CancellationToken ct);

    Task EnsureCanReadAntenatalRecordAsync(
        Guid antenatalRecordId,
        Guid patientId,
        Guid obstetricDoctorId,
        CancellationToken ct);
}
