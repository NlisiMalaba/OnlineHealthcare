using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RegisterDoctor;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Identity;

public sealed class DoctorRegistrationWorkflowTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task RegisterDoctor_CreatesPendingDoctorAndQueuesLicenseVerification()
    {
        var response = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        var doctor = await _host.DbContext.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleAsync(d => d.Id == response.DoctorId);

        Assert.Equal(DoctorVerificationStatus.Pending, doctor.VerificationStatus);
        Assert.Equal("pending", response.VerificationStatus);
        Assert.NotEmpty(doctor.AvailabilitySlots);
        Assert.False(string.IsNullOrWhiteSpace(doctor.CredentialsStorageKey));

        var queueItem = await _host.DbContext.LicenseVerificationQueue
            .SingleAsync(q => q.DoctorId == doctor.Id);

        Assert.False(queueItem.IsCompleted);
    }

    [Fact]
    public async Task RegisterDoctor_WithDuplicateLicense_ReturnsIdentityConflict()
    {
        var license = $"HPCZ-DUP-{Guid.NewGuid():N}"[..20];
        await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(licenseNumber: license),
            CancellationToken.None);

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            _host.Sender.Send(
                DoctorRegistrationTestData.CreateValidCommand(licenseNumber: license),
                CancellationToken.None));

        Assert.Equal(IdentityErrorCodes.IdentityConflict, ex.Code);
    }
}
