using HealthPlatform.Application.MentalHealth.MoodLogs;
using HealthPlatform.Domain.MentalHealth;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class ConsecutiveLowMoodPromptRepository(ApplicationDbContext db)
    : IConsecutiveLowMoodPromptRepository
{
    public Task<bool> ExistsForTriggeringMoodLogAsync(
        Guid patientId,
        string triggeringMoodLogId,
        CancellationToken ct) =>
        db.ConsecutiveLowMoodPrompts.AnyAsync(
            prompt => prompt.PatientId == patientId
                && prompt.TriggeringMoodLogId == triggeringMoodLogId,
            ct);

    public async Task AddAsync(ConsecutiveLowMoodPrompt prompt, CancellationToken ct) =>
        await db.ConsecutiveLowMoodPrompts.AddAsync(prompt, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
