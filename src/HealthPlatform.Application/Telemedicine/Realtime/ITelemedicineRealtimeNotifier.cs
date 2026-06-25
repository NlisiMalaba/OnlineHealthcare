namespace HealthPlatform.Application.Telemedicine.Realtime;

public interface ITelemedicineRealtimeNotifier
{
    Task PublishDurationTickAsync(TelemedicineDurationTickDto tick, CancellationToken ct);

    Task PublishChatMessageAsync(TelemedicineChatMessageDto message, CancellationToken ct);

    Task PublishFileSharedAsync(TelemedicineFileSharedDto file, CancellationToken ct);

    Task PublishReconnectionAttemptingAsync(TelemedicineReconnectionAttemptingDto attempt, CancellationToken ct);

    Task PublishReconnectionSucceededAsync(TelemedicineReconnectionSucceededDto success, CancellationToken ct);

    Task PublishReconnectionPromptRequiredAsync(TelemedicineReconnectionPromptRequiredDto prompt, CancellationToken ct);
}
