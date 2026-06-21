using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RegisterPharmacy;
using HealthPlatform.Application.Identity.UpdatePharmacyProfile;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Identity;

public sealed class PharmacyProfileUpdateWorkflowTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task UpdateProfile_UpdatesNameAddressAndPhone()
    {
        var userId = await RegisterPharmacyAndSetCurrentUserAsync();

        var profile = await _host.Sender.Send(
            new UpdatePharmacyProfileCommand(
                "Updated Pharmacy Name",
                "99 Nelson Mandela Ave, Harare",
                -17.8252,
                31.0335,
                $"+2637{Random.Shared.Next(10_000_000, 99_999_999)}",
                null),
            CancellationToken.None);

        var pharmacy = await _host.DbContext.Pharmacies.SingleAsync(p => p.UserId == userId);

        Assert.Equal("Updated Pharmacy Name", profile.Name);
        Assert.Equal("99 Nelson Mandela Ave, Harare", profile.Address);
        Assert.Equal(-17.8252, profile.Latitude);
        Assert.Equal(31.0335, profile.Longitude);
        Assert.Equal(profile.ContactPhone, pharmacy.ContactPhone);
    }

    private async Task<Guid> RegisterPharmacyAndSetCurrentUserAsync()
    {
        var registration = await _host.Sender.Send(
            PharmacyRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        var pharmacy = await _host.DbContext.Pharmacies.SingleAsync(p => p.Id == registration.PharmacyId);
        _host.CurrentUser.UserId = pharmacy.UserId;
        return pharmacy.UserId;
    }
}
