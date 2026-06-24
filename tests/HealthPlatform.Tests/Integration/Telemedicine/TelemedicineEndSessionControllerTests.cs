using HealthPlatform.API.Controllers;
using HealthPlatform.Application.Telemedicine.EndSession;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace HealthPlatform.Tests.Integration.Telemedicine;

public sealed class TelemedicineEndSessionControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task End_endpoint_returns_ended_session_with_summary_reference()
    {
        var context = await TelemedicineSessionTestContextFactory.CreateActiveAsync(_host);
        _host.CurrentUser.UserId = context.DoctorUserId;

        var controller = new TelemedicineSessionsController(_host.Sender);
        var result = await controller.EndAsync(context.AppointmentId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<EndTelemedicineSessionDto>(ok.Value);
        Assert.Equal("ended", payload.Status);
        Assert.NotNull(payload.SessionSummaryRef);
    }
}
