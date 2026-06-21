using FluentValidation;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RegisterDoctor;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.RegisterPharmacy;
using HealthPlatform.Application.Identity.RejectDoctorLicense;
using HealthPlatform.Application.Identity.UpdateDoctorProfile;
using HealthPlatform.Application.Identity.UpdatePatientProfile;
using HealthPlatform.Application.Identity.UpdatePharmacyProfile;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Identity;

/// <summary>
/// Targeted unit tests for registration conflicts, pending verification state, and profile CRUD edge cases.
/// Requirements: 1.6, 2.2, 2.7
/// </summary>
public sealed class IdentityEdgeCaseTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    // Requirement 1.6 — duplicate registration rejection

    [Fact]
    public async Task RegisterPatient_WithDuplicateEmail_ReturnsIdentityConflict()
    {
        var email = $"dup-email-{Guid.NewGuid():N}@example.com";
        var command = new RegisterPatientCommand(
            PatientAuthProvider.Email,
            "Jane Doe",
            null,
            email,
            PatientRegistrationTestHost.ValidPassword,
            null);

        await _host.Sender.Send(command, CancellationToken.None);

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            _host.Sender.Send(command, CancellationToken.None));

        Assert.Equal(IdentityErrorCodes.IdentityConflict, ex.Code);
    }

    [Fact]
    public async Task RegisterDoctor_WithDuplicateEmail_ReturnsIdentityConflict()
    {
        var email = $"doctor-dup-{Guid.NewGuid():N}@example.com";
        await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(email: email),
            CancellationToken.None);

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            _host.Sender.Send(
                DoctorRegistrationTestData.CreateValidCommand(email: email),
                CancellationToken.None));

        Assert.Equal(IdentityErrorCodes.IdentityConflict, ex.Code);
    }

    [Fact]
    public async Task RegisterDoctor_WithDuplicatePhone_ReturnsIdentityConflict()
    {
        var phone = $"+2637{Random.Shared.Next(10_000_000, 99_999_999)}";
        await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(phoneNumber: phone),
            CancellationToken.None);

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            _host.Sender.Send(
                DoctorRegistrationTestData.CreateValidCommand(phoneNumber: phone),
                CancellationToken.None));

        Assert.Equal(IdentityErrorCodes.IdentityConflict, ex.Code);
    }

    [Fact]
    public async Task RegisterPharmacy_WithDuplicatePhone_ReturnsIdentityConflict()
    {
        var phone = $"+2637{Random.Shared.Next(10_000_000, 99_999_999)}";
        await _host.Sender.Send(
            PharmacyRegistrationTestData.CreateValidCommand(phoneNumber: phone),
            CancellationToken.None);

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            _host.Sender.Send(
                PharmacyRegistrationTestData.CreateValidCommand(phoneNumber: phone),
                CancellationToken.None));

        Assert.Equal(IdentityErrorCodes.IdentityConflict, ex.Code);
    }

    // Requirement 2.2 — doctor registration starts in pending state

    [Fact]
    public async Task RegisterDoctor_ResponseAndEntityArePending()
    {
        var response = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        var doctor = await _host.DbContext.Doctors
            .AsNoTracking()
            .SingleAsync(d => d.Id == response.DoctorId);

        Assert.Equal("pending", response.VerificationStatus);
        Assert.Equal(DoctorVerificationStatus.Pending, doctor.VerificationStatus);
    }

    // Requirement 2.7 — license rejection and verification edge cases

    [Fact]
    public async Task RejectDoctorLicense_WhenAlreadyRejected_ReturnsInvalidVerificationState()
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        await _host.Sender.Send(
            new RejectDoctorLicenseCommand(registration.DoctorId, "Initial rejection reason."),
            CancellationToken.None);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _host.Sender.Send(
                new RejectDoctorLicenseCommand(registration.DoctorId, "Second rejection attempt."),
                CancellationToken.None));

        Assert.Equal(IdentityErrorCodes.InvalidVerificationState, ex.Code);
    }

    [Fact]
    public async Task VerifyDoctorLicense_AfterRejection_ReturnsInvalidVerificationState()
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        await _host.Sender.Send(
            new RejectDoctorLicenseCommand(registration.DoctorId, "License could not be verified."),
            CancellationToken.None);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _host.Sender.Send(
                new VerifyDoctorLicenseCommand(registration.DoctorId),
                CancellationToken.None));

        Assert.Equal(IdentityErrorCodes.InvalidVerificationState, ex.Code);
    }

    // Profile CRUD edge cases

    [Fact]
    public async Task UpdatePatientProfile_WithoutAuthenticatedUser_ReturnsAccessDenied()
    {
        _host.CurrentUser.UserId = null;

        var ex = await Assert.ThrowsAsync<AccessDeniedException>(() =>
            _host.Sender.Send(
                new UpdatePatientProfileCommand("Name", null, null, null, null, null),
                CancellationToken.None));

        Assert.Equal("ACCESS_DENIED", ex.Code);
    }

    [Fact]
    public async Task UpdatePatientProfile_WhenPatientNotFound_ReturnsNotFound()
    {
        _host.CurrentUser.UserId = Guid.CreateVersion7();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _host.Sender.Send(
                new UpdatePatientProfileCommand("Name", null, null, null, null, null),
                CancellationToken.None));

        Assert.Equal("PATIENT_NOT_FOUND", ex.Code);
    }

    [Fact]
    public async Task UpdateDoctorProfile_WhenDoctorNotFound_ReturnsNotFound()
    {
        _host.CurrentUser.UserId = Guid.CreateVersion7();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _host.Sender.Send(
                new UpdateDoctorProfileCommand(10m, null, null, null, null, null),
                CancellationToken.None));

        Assert.Equal(IdentityErrorCodes.DoctorNotFound, ex.Code);
    }

    [Fact]
    public async Task UpdatePharmacyProfile_WhenPharmacyNotFound_ReturnsNotFound()
    {
        _host.CurrentUser.UserId = Guid.CreateVersion7();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _host.Sender.Send(
                new UpdatePharmacyProfileCommand("Name", null, null, null, null, null),
                CancellationToken.None));

        Assert.Equal(IdentityErrorCodes.PharmacyNotFound, ex.Code);
    }

    [Fact]
    public async Task UpdatePharmacyProfile_WithDuplicatePhone_ReturnsIdentityConflict()
    {
        var firstPhone = $"+2637{Random.Shared.Next(10_000_000, 99_999_999)}";
        var secondPhone = $"+2637{Random.Shared.Next(10_000_000, 99_999_999)}";

        var first = await _host.Sender.Send(
            PharmacyRegistrationTestData.CreateValidCommand(phoneNumber: firstPhone),
            CancellationToken.None);
        var second = await _host.Sender.Send(
            PharmacyRegistrationTestData.CreateValidCommand(phoneNumber: secondPhone),
            CancellationToken.None);

        var secondPharmacy = await _host.DbContext.Pharmacies.SingleAsync(p => p.Id == second.PharmacyId);
        _host.CurrentUser.UserId = secondPharmacy.UserId;

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            _host.Sender.Send(
                new UpdatePharmacyProfileCommand(null, null, null, null, firstPhone, null),
                CancellationToken.None));

        Assert.Equal(IdentityErrorCodes.IdentityConflict, ex.Code);
    }

    [Fact]
    public async Task UpdatePatientProfile_WithEmptyPayload_RejectedByValidator()
    {
        var registration = await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Edge Case Patient",
                null,
                $"edge-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.SingleAsync(p => p.Id == registration.PatientId);
        _host.CurrentUser.UserId = patient.UserId;

        await Assert.ThrowsAsync<ValidationException>(() =>
            _host.Sender.Send(
                new UpdatePatientProfileCommand(null, null, null, null, null, null),
                CancellationToken.None));
    }
}
