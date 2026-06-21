using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RejectDoctorLicense;
using HealthPlatform.Application.Identity.RegisterDoctor;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Identity;

public sealed class LicenseVerificationWorkflowTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task VerifyDoctorLicense_TransitionsToVerifiedAndCompletesQueue()
    {
        var registration = await RegisterPendingDoctorAsync();

        var result = await _host.Sender.Send(
            new VerifyDoctorLicenseCommand(registration.DoctorId),
            CancellationToken.None);

        var doctor = await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
        var queueItem = await _host.DbContext.LicenseVerificationQueue
            .SingleAsync(q => q.DoctorId == registration.DoctorId);

        Assert.Equal("verified", result.VerificationStatus);
        Assert.Equal(DoctorVerificationStatus.Verified, doctor.VerificationStatus);
        Assert.Null(doctor.RejectionReason);
        Assert.True(queueItem.IsCompleted);
    }

    [Fact]
    public async Task RejectDoctorLicense_TransitionsToRejectedWithReasonAndCompletesQueue()
    {
        var registration = await RegisterPendingDoctorAsync();
        const string reason = "License number could not be validated with HPCZ registry.";

        var result = await _host.Sender.Send(
            new RejectDoctorLicenseCommand(registration.DoctorId, reason),
            CancellationToken.None);

        var doctor = await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
        var queueItem = await _host.DbContext.LicenseVerificationQueue
            .SingleAsync(q => q.DoctorId == registration.DoctorId);

        Assert.Equal("rejected", result.VerificationStatus);
        Assert.Equal(reason, result.RejectionReason);
        Assert.Equal(DoctorVerificationStatus.Rejected, doctor.VerificationStatus);
        Assert.Equal(reason, doctor.RejectionReason);
        Assert.True(queueItem.IsCompleted);
    }

    [Fact]
    public async Task VerifyDoctorLicense_WhenAlreadyVerified_ReturnsInvalidVerificationState()
    {
        var registration = await RegisterPendingDoctorAsync();
        await _host.Sender.Send(
            new VerifyDoctorLicenseCommand(registration.DoctorId),
            CancellationToken.None);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _host.Sender.Send(
                new VerifyDoctorLicenseCommand(registration.DoctorId),
                CancellationToken.None));

        Assert.Equal(IdentityErrorCodes.InvalidVerificationState, ex.Code);
    }

    [Fact]
    public async Task RejectDoctorLicense_WhenDoctorNotFound_ReturnsNotFound()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _host.Sender.Send(
                new RejectDoctorLicenseCommand(Guid.CreateVersion7(), "Invalid license record."),
                CancellationToken.None));

        Assert.Equal(IdentityErrorCodes.DoctorNotFound, ex.Code);
    }

    private Task<DoctorRegistrationResponseDto> RegisterPendingDoctorAsync() =>
        _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);
}
