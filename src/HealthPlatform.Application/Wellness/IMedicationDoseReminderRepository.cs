using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness;

public interface IMedicationDoseReminderRepository
{
    Task<IReadOnlyList<DueMedicationDose>> ListDueDosesAsync(
        DateTime nowUtc,
        TimeSpan lookbackWindow,
        int batchSize,
        CancellationToken ct);

    Task AddSentReminderAsync(MedicationDoseReminder reminder, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
