using HealthPlatform.Application.Telemedicine.Realtime;
using HealthPlatform.Application.Telemedicine.Realtime.Chat;
using HealthPlatform.Application.Telemedicine.Realtime.ConnectSession;
using HealthPlatform.Application.Telemedicine.Realtime.Reconnection;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HealthPlatform.API.Hubs;

[Authorize]
public sealed class TelemedicineHub(ISender sender) : Hub
{
    public const string AppointmentIdContextKey = "TelemedicineAppointmentId";

    public async Task JoinSession(Guid appointmentId)
    {
        var connection = await sender.Send(new ConnectTelemedicineSessionCommand(appointmentId));
        Context.Items[AppointmentIdContextKey] = appointmentId;
        await Groups.AddToGroupAsync(Context.ConnectionId, connection.GroupName);
    }

    public Task<TelemedicineChatMessageDto> SendChatMessage(Guid appointmentId, string message) =>
        sender.Send(new SendTelemedicineChatMessageCommand(appointmentId, message));

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items.TryGetValue(AppointmentIdContextKey, out var appointmentIdValue)
            && appointmentIdValue is Guid appointmentId)
        {
            await sender.Send(new NotifyTelemedicineParticipantDisconnectedCommand(appointmentId));
        }

        await base.OnDisconnectedAsync(exception);
    }
}
