using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Telemedicine;
using HealthPlatform.Application.Telemedicine.Realtime.Files;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace HealthPlatform.Tests.Unit.Telemedicine;

public sealed class ShareTelemedicineSessionFileCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Handle_Publishes_file_shared_event_for_doctor()
    {
        var context = await TelemedicineSessionTestContextFactory.CreateActiveAsync(_host);
        _host.CurrentUser.UserId = context.DoctorUserId;

        await using var stream = new MemoryStream([0x25, 0x50, 0x44, 0x46]);
        var shared = await _host.Sender.Send(
            new ShareTelemedicineSessionFileCommand(
                context.AppointmentId,
                stream,
                "application/pdf",
                "lab-summary.pdf",
                4),
            CancellationToken.None);

        Assert.Equal(context.AppointmentId, shared.AppointmentId);
        Assert.Equal("lab-summary.pdf", shared.FileName);
        Assert.Single(_host.TelemedicineRealtimeNotifier.SharedFiles);
    }

    [Fact]
    public async Task Handle_Rejects_patient_file_share()
    {
        var context = await TelemedicineSessionTestContextFactory.CreateActiveAsync(_host);
        _host.CurrentUser.UserId = context.PatientUserId;

        await using var stream = new MemoryStream([0x25, 0x50, 0x44, 0x46]);

        var exception = await Assert.ThrowsAsync<AccessDeniedException>(() =>
            _host.Sender.Send(
                new ShareTelemedicineSessionFileCommand(
                    context.AppointmentId,
                    stream,
                    "application/pdf",
                    "notes.pdf",
                    4),
                CancellationToken.None));

        Assert.Equal(TelemedicineErrorCodes.FileShareNotAllowed, exception.Code);
    }
}
