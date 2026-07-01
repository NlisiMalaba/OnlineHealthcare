using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness;

public interface IMedicationScheduleRepository
{
    Task<MedicationSchedule?> GetByPrescriptionIdAsync(Guid prescriptionId, CancellationToken ct);

    Task<MedicationSchedule?> GetActiveByIdForPatientAsync(
        Guid scheduleId,
        Guid patientId,
        CancellationToken ct);

    Task<IReadOnlyList<MedicationSchedule>> ListActiveByPatientIdAsync(Guid patientId, CancellationToken ct);

    Task AddAsync(MedicationSchedule schedule, CancellationToken ct);
}
