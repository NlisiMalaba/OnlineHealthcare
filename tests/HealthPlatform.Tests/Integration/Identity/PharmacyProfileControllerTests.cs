using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Identity;
using HealthPlatform.Application.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Identity;

public sealed class PharmacyProfileControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task UpdateProfile_ReturnsUpdatedPharmacyProfile()
    {
        var registration = await _host.Sender.Send(
            PharmacyRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        var pharmacy = await _host.DbContext.Pharmacies.SingleAsync(p => p.Id == registration.PharmacyId);
        _host.CurrentUser.UserId = pharmacy.UserId;

        var controller = new PharmacyProfileController(_host.Sender);
        var result = await controller.UpdateProfileAsync(
            new UpdatePharmacyProfileRequest
            {
                Name = "Controller Updated Pharmacy",
                Address = "8 Leopold Takawira, Bulawayo",
                Latitude = -20.1500,
                Longitude = 28.5800
            },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var profile = Assert.IsType<PharmacyProfileDto>(ok.Value);
        Assert.Equal("Controller Updated Pharmacy", profile.Name);
        Assert.Equal("8 Leopold Takawira, Bulawayo", profile.Address);
    }
}
