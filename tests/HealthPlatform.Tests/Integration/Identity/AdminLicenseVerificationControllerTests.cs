using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Identity;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Identity;

public sealed class AdminLicenseVerificationControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task VerifyDoctorLicense_ReturnsVerifiedStatus()
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        var controller = CreateController();
        var result = await controller.VerifyDoctorLicenseAsync(registration.DoctorId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<LicenseVerificationResultDto>(ok.Value);
        Assert.Equal("verified", payload.VerificationStatus);
        Assert.Null(payload.RejectionReason);

        var doctor = await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
        Assert.Equal(DoctorVerificationStatus.Verified, doctor.VerificationStatus);
    }

    [Fact]
    public async Task RejectDoctorLicense_ReturnsRejectedStatusWithReason()
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        const string reason = "Submitted credentials did not match the license number.";
        var controller = CreateController();
        var result = await controller.RejectDoctorLicenseAsync(
            registration.DoctorId,
            new RejectDoctorLicenseRequest { Reason = reason },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<LicenseVerificationResultDto>(ok.Value);
        Assert.Equal("rejected", payload.VerificationStatus);
        Assert.Equal(reason, payload.RejectionReason);
    }

    private AdminLicenseVerificationController CreateController() =>
        new(_host.Sender)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
}
