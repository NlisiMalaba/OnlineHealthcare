using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Telemedicine;
using HealthPlatform.Application.Telemedicine.RecordingConsent;
using HealthPlatform.Domain.Telemedicine;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Telemedicine;

public sealed class EnableSessionRecordingCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Handle_Rejects_enable_without_patient_consent()
    {
        var context = await TelemedicineSessionTestContextFactory.CreateAsync(_host);
        _host.CurrentUser.UserId = context.DoctorUserId;

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _host.Sender.Send(new EnableSessionRecordingCommand(context.AppointmentId), CancellationToken.None));

        Assert.Equal(TelemedicineErrorCodes.RecordingConsentRequired, exception.Code);

        var session = await _host.DbContext.TelemedicineSessions.SingleAsync();
        Assert.False(session.RecordingEnabled);
    }

    [Fact]
    public async Task Handle_Enables_recording_only_when_consent_granted()
    {
        var context = await TelemedicineSessionTestContextFactory.CreateAsync(_host);

        await _host.Sender.Send(
            new GrantRecordingConsentCommand(context.AppointmentId),
            CancellationToken.None);

        _host.CurrentUser.UserId = context.DoctorUserId;

        var response = await _host.Sender.Send(
            new EnableSessionRecordingCommand(context.AppointmentId),
            CancellationToken.None);

        Assert.True(response.RecordingConsent);
        Assert.True(response.RecordingEnabled);

        var session = await _host.DbContext.TelemedicineSessions.SingleAsync();
        Assert.True(session.RecordingConsent);
        Assert.True(session.RecordingEnabled);
    }
}
