using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Identity;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.UpdatePatientProfile;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace HealthPlatform.Tests.Integration.Identity;

public sealed class PatientProfileControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task UpdateProfile_ReturnsUpdatedPatientProfile()
    {
        var registration = await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Controller Original",
                null,
                $"controller-profile-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.FindAsync(registration.PatientId);
        Assert.NotNull(patient);
        _host.CurrentUser.UserId = patient!.UserId;

        var controller = new PatientProfileController(_host.Sender);
        var result = await controller.UpdateProfileAsync(
            new UpdatePatientProfileRequest
            {
                FullName = "Controller Updated",
                DateOfBirth = new DateOnly(1988, 12, 1),
                BloodType = BloodType.BNegative,
                KnownAllergies = ["Dust"],
                ChronicConditions = ["Diabetes"]
            },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var profile = Assert.IsType<PatientProfileDto>(ok.Value);
        Assert.Equal("Controller Updated", profile.FullName);
        Assert.Equal(BloodType.BNegative, profile.BloodType);
    }
}
