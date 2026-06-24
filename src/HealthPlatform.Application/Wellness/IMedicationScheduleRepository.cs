using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness;

public interface IMedicationScheduleRepository
{
    Task<IReadOnlyList<MedicationSchedule>> ListActiveByPatientIdAsync(Guid patientId, CancellationToken ct);

    Task AddAsync(MedicationSchedule schedule, CancellationToken ct);
}
