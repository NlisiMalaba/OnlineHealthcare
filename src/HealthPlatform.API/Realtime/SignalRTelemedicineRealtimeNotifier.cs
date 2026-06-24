using HealthPlatform.API.Hubs;
using HealthPlatform.Application.Telemedicine;
using HealthPlatform.Application.Telemedicine.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace HealthPlatform.API.Realtime;

public sealed class SignalRTelemedicineRealtimeNotifier(IHubContext<TelemedicineHub> hubContext)
    : ITelemedicineRealtimeNotifier
{
    public Task PublishDurationTickAsync(TelemedicineDurationTickDto tick, CancellationToken ct) =>
        hubContext.Clients
            .Group(TelemedicineSessionGroupNames.ForAppointment(tick.AppointmentId))
            .SendAsync(TelemedicineHubEvents.DurationTick, tick, ct);

    public Task PublishChatMessageAsync(TelemedicineChatMessageDto message, CancellationToken ct) =>
        hubContext.Clients
            .Group(TelemedicineSessionGroupNames.ForAppointment(message.AppointmentId))
            .SendAsync(TelemedicineHubEvents.ChatMessageReceived, message, ct);

    public Task PublishFileSharedAsync(TelemedicineFileSharedDto file, CancellationToken ct) =>
        hubContext.Clients
            .Group(TelemedicineSessionGroupNames.ForAppointment(file.AppointmentId))
            .SendAsync(TelemedicineHubEvents.FileShared, file, ct);
}
