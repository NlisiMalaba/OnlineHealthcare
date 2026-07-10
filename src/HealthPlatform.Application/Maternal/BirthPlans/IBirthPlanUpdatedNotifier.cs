namespace HealthPlatform.Application.Maternal.BirthPlans;

public interface IBirthPlanUpdatedNotifier
{
    Task NotifyBirthPlanUpdatedAsync(
        Guid obstetricDoctorUserId,
        Guid birthPlanId,
        Guid antenatalRecordId,
        Guid patientId,
        CancellationToken ct);
}
