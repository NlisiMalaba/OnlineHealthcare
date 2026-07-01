using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness;

public sealed record OverdueMedicationDose(
    Guid ScheduleId,
    Guid PatientId,
    DateTime ScheduledAtUtc);

public interface IAdherenceEventRepository
{
    Task<AdherenceEvent?> GetByScheduleAndScheduledAtAsync(
        Guid scheduleId,
        DateTime scheduledAtUtc,
        CancellationToken ct);

    Task<IReadOnlyList<OverdueMedicationDose>> ListOverdueUnconfirmedDosesAsync(
        DateTime nowUtc,
        int batchSize,
        CancellationToken ct);

    Task<IReadOnlyList<AdherenceEvent>> ListByPatientIdOrderedByScheduledDescAsync(
        Guid patientId,
        int take,
        CancellationToken ct);

    Task<int> CountRecordedByScheduleIdAsync(Guid scheduleId, CancellationToken ct);

    Task<IReadOnlyList<AdherenceEvent>> ListByScheduleIdsInRangeAsync(
        IReadOnlyCollection<Guid> scheduleIds,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken ct);

    Task AddAsync(AdherenceEvent adherenceEvent, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
