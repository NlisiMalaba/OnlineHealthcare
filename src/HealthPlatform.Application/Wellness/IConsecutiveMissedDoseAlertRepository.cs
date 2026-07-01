using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness;

public interface IConsecutiveMissedDoseAlertRepository
{
    Task<bool> ExistsForStreakAsync(
        Guid patientId,
        DateTime streakEndScheduledAtUtc,
        CancellationToken ct);

    Task AddAsync(ConsecutiveMissedDoseAlert alert, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
