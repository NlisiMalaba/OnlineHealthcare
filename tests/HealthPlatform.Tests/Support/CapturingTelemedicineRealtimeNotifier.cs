using HealthPlatform.Application.Telemedicine.Realtime;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingTelemedicineRealtimeNotifier : ITelemedicineRealtimeNotifier
{
    public List<TelemedicineDurationTickDto> DurationTicks { get; } = [];

    public List<TelemedicineChatMessageDto> ChatMessages { get; } = [];

    public List<TelemedicineFileSharedDto> SharedFiles { get; } = [];

    public List<TelemedicineReconnectionAttemptingDto> ReconnectionAttempts { get; } = [];

    public List<TelemedicineReconnectionSucceededDto> ReconnectionSuccesses { get; } = [];

    public List<TelemedicineReconnectionPromptRequiredDto> ReconnectionPrompts { get; } = [];

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

    public Task PublishReconnectionAttemptingAsync(TelemedicineReconnectionAttemptingDto attempt, CancellationToken ct)
    {
        ReconnectionAttempts.Add(attempt);
        return Task.CompletedTask;
    }

    public Task PublishReconnectionSucceededAsync(TelemedicineReconnectionSucceededDto success, CancellationToken ct)
    {
        ReconnectionSuccesses.Add(success);
        return Task.CompletedTask;
    }

    public Task PublishReconnectionPromptRequiredAsync(
        TelemedicineReconnectionPromptRequiredDto prompt,
        CancellationToken ct)
    {
        ReconnectionPrompts.Add(prompt);
        return Task.CompletedTask;
    }
}
