using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Appointments.EventHandlers;
using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Telemedicine.EventHandlers;
using HealthPlatform.Application.Telemedicine;
using HealthPlatform.Application.Telemedicine.RecordingConsent;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Telemedicine;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Telemedicine;

public sealed class GrantRecordingConsentCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Handle_Persists_recording_consent_before_session_starts()
    {
        var context = await TelemedicineSessionTestContextFactory.CreateAsync(_host);

        var response = await _host.Sender.Send(
            new GrantRecordingConsentCommand(context.AppointmentId),
            CancellationToken.None);

        Assert.True(response.RecordingConsent);
        Assert.False(response.RecordingEnabled);

        var session = await _host.DbContext.TelemedicineSessions.SingleAsync();
        Assert.True(session.RecordingConsent);
        Assert.False(session.RecordingEnabled);
    }

    [Fact]
    public async Task Handle_Rejects_consent_after_session_has_started()
    {
        var context = await TelemedicineSessionTestContextFactory.CreateAsync(_host);
        _host.CurrentUser.UserId = context.PatientUserId;

        await _host.Sender.Send(
            new Application.Telemedicine.JoinSession.JoinTelemedicineSessionCommand(context.AppointmentId, null),
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _host.Sender.Send(new GrantRecordingConsentCommand(context.AppointmentId), CancellationToken.None));

        Assert.Equal(TelemedicineErrorCodes.RecordingConsentNotAllowed, exception.Code);
    }
}
