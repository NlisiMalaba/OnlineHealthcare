using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Security;
using HealthPlatform.Application.Telemedicine;
using HealthPlatform.Application.Telemedicine.Realtime.Chat;
using HealthPlatform.Application.Telemedicine.Realtime.ConnectSession;
using HealthPlatform.Tests.Support;
using Xunit;

namespace HealthPlatform.Tests.Unit.Telemedicine;

public sealed class SendTelemedicineChatMessageCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Handle_Publishes_chat_message_for_active_session()
    {
        var context = await TelemedicineSessionTestContextFactory.CreateActiveAsync(_host);
        _host.CurrentUser.UserId = context.PatientUserId;

        var message = await _host.Sender.Send(
            new SendTelemedicineChatMessageCommand(context.AppointmentId, "Hello doctor"),
            CancellationToken.None);

        Assert.Equal(context.AppointmentId, message.AppointmentId);
        Assert.Equal(ApplicationRoles.Patient, message.SenderRole);
        Assert.Single(_host.TelemedicineRealtimeNotifier.ChatMessages);
    }

    [Fact]
    public async Task Handle_Rejects_chat_before_session_is_active()
    {
        var context = await TelemedicineSessionTestContextFactory.CreateAsync(_host);
        _host.CurrentUser.UserId = context.PatientUserId;

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _host.Sender.Send(
                new SendTelemedicineChatMessageCommand(context.AppointmentId, "Too early"),
                CancellationToken.None));

        Assert.Equal(TelemedicineErrorCodes.SessionNotActive, exception.Code);
    }

    [Fact]
    public async Task Connect_returns_session_group_name()
    {
        var context = await TelemedicineSessionTestContextFactory.CreateAsync(_host);
        _host.CurrentUser.UserId = context.DoctorUserId;

        var connection = await _host.Sender.Send(
            new ConnectTelemedicineSessionCommand(context.AppointmentId),
            CancellationToken.None);

        Assert.Equal(
            TelemedicineSessionGroupNames.ForAppointment(context.AppointmentId),
            connection.GroupName);
    }
}
