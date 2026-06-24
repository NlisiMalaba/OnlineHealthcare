using HealthPlatform.Application.Telemedicine.Realtime;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingTelemedicineRealtimeNotifier : ITelemedicineRealtimeNotifier
{
    public List<TelemedicineDurationTickDto> DurationTicks { get; } = [];

    public List<TelemedicineChatMessageDto> ChatMessages { get; } = [];

    public List<TelemedicineFileSharedDto> SharedFiles { get; } = [];

    public Task PublishDurationTickAsync(TelemedicineDurationTickDto tick, CancellationToken ct)
    {
        DurationTicks.Add(tick);
        return Task.CompletedTask;
    }

    public Task PublishChatMessageAsync(TelemedicineChatMessageDto message, CancellationToken ct)
    {
        ChatMessages.Add(message);
        return Task.CompletedTask;
    }

    public Task PublishFileSharedAsync(TelemedicineFileSharedDto file, CancellationToken ct)
    {
        SharedFiles.Add(file);
        return Task.CompletedTask;
    }
}
