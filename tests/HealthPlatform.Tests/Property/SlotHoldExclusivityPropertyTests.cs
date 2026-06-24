using FsCheck;
using FsCheck.Xunit;
using HealthPlatform.Infrastructure.Appointments;

namespace HealthPlatform.Tests.Properties;

public sealed class SlotHoldExclusivityPropertyTests
{
    // Feature: online-healthcare-platform, Property 7: Slot Hold Exclusivity
    [Property(MaxTest = 100)]
    public bool Concurrent_holds_for_same_slot_allow_at_most_one_winner(PositiveInt rawAttemptCount)
    {
        var attemptCount = Math.Clamp(rawAttemptCount.Get, 2, 64);
        var slotId = Guid.CreateVersion7();
        var ttl = TimeSpan.FromMinutes(10);
        var holdService = new InMemorySlotHoldService();

        using var startGate = new ManualResetEventSlim(false);
        var attempts = Enumerable.Range(0, attemptCount)
            .Select(_ => Task.Run(async () =>
            {
                startGate.Wait();
                return await holdService.TryHoldAsync(slotId, Guid.CreateVersion7(), ttl, CancellationToken.None);
            }))
            .ToList();

        startGate.Set();
        Task.WaitAll([.. attempts]);

        var successCount = attempts.Count(task => task.Result);
        if (successCount > 1)
        {
            return false;
        }

        // Hold window is active, so every new attempt must fail.
        var followUpAttempt = holdService
            .TryHoldAsync(slotId, Guid.CreateVersion7(), ttl, CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        return successCount == 1 && !followUpAttempt;
    }
}
