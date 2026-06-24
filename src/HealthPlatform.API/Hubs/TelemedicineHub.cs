using HealthPlatform.Application.Telemedicine.Realtime;
using HealthPlatform.Application.Telemedicine.Realtime.Chat;
using HealthPlatform.Application.Telemedicine.Realtime.ConnectSession;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HealthPlatform.API.Hubs;

[Authorize]
public sealed class TelemedicineHub(ISender sender) : Hub
{
    public async Task JoinSession(Guid appointmentId)
    {
        var connection = await sender.Send(new ConnectTelemedicineSessionCommand(appointmentId));
        await Groups.AddToGroupAsync(Context.ConnectionId, connection.GroupName);
    }

    public Task<TelemedicineChatMessageDto> SendChatMessage(Guid appointmentId, string message) =>
        sender.Send(new SendTelemedicineChatMessageCommand(appointmentId, message));
}
