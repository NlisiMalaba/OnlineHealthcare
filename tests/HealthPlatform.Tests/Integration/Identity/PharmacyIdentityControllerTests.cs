using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Identity;
using HealthPlatform.Application.Identity.RegisterPharmacy;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace HealthPlatform.Tests.Integration.Identity;

public sealed class PharmacyIdentityControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task RegisterPharmacy_ReturnsCreatedWithPendingStatus()
    {
        var controller = new PharmacyIdentityController(_host.Sender)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = await controller.RegisterPharmacyAsync(
            new RegisterPharmacyRequest
            {
                Name = "Controller Pharmacy",
                Address = "22 Fife Street, Bulawayo",
                Latitude = -20.1555,
                Longitude = 28.5845,
                Email = $"pharmacy-controller-{Guid.NewGuid():N}@example.com",
                PhoneNumber = $"+2637{Random.Shared.Next(10_000_000, 99_999_999)}",
                Password = PatientRegistrationTestHost.ValidPassword
            },
            CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result.Result);
        var payload = Assert.IsType<PharmacyRegistrationResponseDto>(created.Value);
        Assert.Equal("pending", payload.VerificationStatus);
        Assert.NotEqual(Guid.Empty, payload.PharmacyId);
    }
}
