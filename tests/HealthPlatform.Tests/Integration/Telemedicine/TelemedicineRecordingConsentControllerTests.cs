using HealthPlatform.API.Controllers;
using HealthPlatform.Application.Telemedicine.RecordingConsent;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace HealthPlatform.Tests.Integration.Telemedicine;

public sealed class TelemedicineRecordingConsentControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Recording_consent_and_enable_endpoints_persist_flags()
    {
        var context = await TelemedicineSessionTestContextFactory.CreateAsync(_host);
        var controller = new TelemedicineSessionsController(_host.Sender);

        var consentResult = await controller.GrantRecordingConsentAsync(context.AppointmentId, CancellationToken.None);
        var consentOk = Assert.IsType<OkObjectResult>(consentResult.Result);
        var consentPayload = Assert.IsType<GrantRecordingConsentDto>(consentOk.Value);
        Assert.True(consentPayload.RecordingConsent);
        Assert.False(consentPayload.RecordingEnabled);

        _host.CurrentUser.UserId = context.DoctorUserId;

        var enableResult = await controller.EnableRecordingAsync(context.AppointmentId, CancellationToken.None);
        var enableOk = Assert.IsType<OkObjectResult>(enableResult.Result);
        var enablePayload = Assert.IsType<EnableSessionRecordingDto>(enableOk.Value);
        Assert.True(enablePayload.RecordingConsent);
        Assert.True(enablePayload.RecordingEnabled);
    }
}
