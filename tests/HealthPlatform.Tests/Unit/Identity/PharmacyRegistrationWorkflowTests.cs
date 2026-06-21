using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RegisterPharmacy;
using HealthPlatform.Application.Identity.UpdatePharmacyProfile;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Identity;

public sealed class PharmacyRegistrationWorkflowTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task RegisterPharmacy_CreatesPendingPharmacy()
    {
        var response = await _host.Sender.Send(
            PharmacyRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        var pharmacy = await _host.DbContext.Pharmacies
            .SingleAsync(p => p.Id == response.PharmacyId);

        Assert.Equal(PharmacyVerificationStatus.Pending, pharmacy.VerificationStatus);
        Assert.Equal("pending", response.VerificationStatus);
    }

    [Fact]
    public async Task RegisterPharmacy_WithDuplicateEmail_ReturnsIdentityConflict()
    {
        var email = $"pharmacy-dup-{Guid.NewGuid():N}@example.com";
        await _host.Sender.Send(
            PharmacyRegistrationTestData.CreateValidCommand(email: email),
            CancellationToken.None);

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            _host.Sender.Send(
                PharmacyRegistrationTestData.CreateValidCommand(email: email),
                CancellationToken.None));

        Assert.Equal(IdentityErrorCodes.IdentityConflict, ex.Code);
    }
}
