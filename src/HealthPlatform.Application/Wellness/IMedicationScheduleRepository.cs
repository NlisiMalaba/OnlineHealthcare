using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness;

public interface IMedicationScheduleRepository
{
    Task<MedicationSchedule?> GetByPrescriptionIdAsync(Guid prescriptionId, CancellationToken ct);

    Task<MedicationSchedule?> GetByIdAsync(Guid scheduleId, CancellationToken ct);

    Task<MedicationSchedule?> GetActiveByIdForPatientAsync(
        Guid scheduleId,
        Guid patientId,
        CancellationToken ct);

    Task<MedicationSchedule?> GetByIdForPatientAsync(
        Guid scheduleId,
        Guid patientId,
        CancellationToken ct);

    Task<IReadOnlyList<MedicationSchedule>> ListActiveByPatientIdAsync(Guid patientId, CancellationToken ct);

    Task<IReadOnlyList<MedicationSchedule>> ListByPatientIdAsync(Guid patientId, CancellationToken ct);

    Task<IReadOnlyList<MedicationSchedule>> ListByPrescriptionIdsAsync(
        IReadOnlyCollection<Guid> prescriptionIds,
        CancellationToken ct);

    Task AddAsync(MedicationSchedule schedule, CancellationToken ct);

    Task UpdateAsync(MedicationSchedule schedule, CancellationToken ct);
}
