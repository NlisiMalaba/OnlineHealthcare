using HealthPlatform.API.Controllers;
using HealthPlatform.Application.Telemedicine.Realtime;
using HealthPlatform.Application.Telemedicine.Realtime.Files;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace HealthPlatform.Tests.Integration.Telemedicine;

public sealed class TelemedicineFileShareControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Share_file_endpoint_publishes_realtime_event()
    {
        var context = await TelemedicineSessionTestContextFactory.CreateActiveAsync(_host);
        _host.CurrentUser.UserId = context.DoctorUserId;

        var file = new FormFile(new MemoryStream([0x89, 0x50, 0x4E, 0x47]), 0, 4, "file", "scan.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var controller = new TelemedicineSessionsController(_host.Sender);
        var result = await controller.ShareFileAsync(context.AppointmentId, file, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<TelemedicineFileSharedDto>(ok.Value);
        Assert.Equal("scan.png", payload.FileName);
        Assert.Single(_host.TelemedicineRealtimeNotifier.SharedFiles);
    }
}
