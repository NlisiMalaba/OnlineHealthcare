namespace HealthPlatform.Application.Telemedicine.Realtime;

public interface ITelemedicineRealtimeNotifier
{
    Task PublishDurationTickAsync(TelemedicineDurationTickDto tick, CancellationToken ct);

    Task PublishChatMessageAsync(TelemedicineChatMessageDto message, CancellationToken ct);

    Task PublishFileSharedAsync(TelemedicineFileSharedDto file, CancellationToken ct);
}
