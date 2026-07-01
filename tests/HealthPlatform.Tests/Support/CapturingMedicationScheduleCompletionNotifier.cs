using HealthPlatform.Application.Wellness;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingMedicationScheduleCompletionNotifier : IMedicationScheduleCompletionNotifier
{
    public List<MedicationScheduleCompletionNotice> Calls { get; } = [];

    public Task NotifyScheduleCompletedAsync(MedicationScheduleCompletionNotice notice, CancellationToken ct)
    {
        Calls.Add(notice);
        return Task.CompletedTask;
    }
}
