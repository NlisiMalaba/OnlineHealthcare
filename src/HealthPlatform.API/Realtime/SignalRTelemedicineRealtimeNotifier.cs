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

    public Task PublishReconnectionAttemptingAsync(TelemedicineReconnectionAttemptingDto attempt, CancellationToken ct) =>
        hubContext.Clients
            .Group(TelemedicineSessionGroupNames.ForAppointment(attempt.AppointmentId))
            .SendAsync(TelemedicineHubEvents.ReconnectionAttempting, attempt, ct);

    public Task PublishReconnectionSucceededAsync(TelemedicineReconnectionSucceededDto success, CancellationToken ct) =>
        hubContext.Clients
            .Group(TelemedicineSessionGroupNames.ForAppointment(success.AppointmentId))
            .SendAsync(TelemedicineHubEvents.ReconnectionSucceeded, success, ct);

    public Task PublishReconnectionPromptRequiredAsync(
        TelemedicineReconnectionPromptRequiredDto prompt,
        CancellationToken ct) =>
        hubContext.Clients
            .Group(TelemedicineSessionGroupNames.ForAppointment(prompt.AppointmentId))
            .SendAsync(TelemedicineHubEvents.ReconnectionPromptRequired, prompt, ct);
}
