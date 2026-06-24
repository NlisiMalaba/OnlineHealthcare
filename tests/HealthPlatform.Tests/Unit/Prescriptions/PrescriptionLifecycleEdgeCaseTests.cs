using FluentValidation.TestHelper;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Prescriptions;
using HealthPlatform.Application.Prescriptions.CancelPrescription;
using HealthPlatform.Application.Prescriptions.CreatePrescription;
using HealthPlatform.Application.Prescriptions.Dispensing;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Prescriptions;
using HealthPlatform.Infrastructure.Prescriptions;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Prescriptions;

public sealed class PrescriptionLifecycleEdgeCaseTests : IAsyncLifetime
{
    private FakeTimeProvider _timeProvider = null!;
    private PatientRegistrationTestHost _host = null!;

    public Task InitializeAsync()
    {
        _timeProvider = new FakeTimeProvider(new DateTime(2026, 6, 24, 10, 0, 0, DateTimeKind.Utc));
        _host = new PatientRegistrationTestHost(timeProvider: _timeProvider);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Dispense_at_exact_expiry_boundary_returns_prescription_expired()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var patient = await SeedPatientAsync();
        var issuedAtUtc = _timeProvider.GetUtcNow().UtcDateTime.AddDays(-PrescriptionPolicies.DefaultExpiryDays);
        var prescription = await IssuePrescriptionAsync(doctor, patient, issuedAtUtc);

        _host.CurrentUser.UserId = patient.UserId;
        var ex = await Assert.ThrowsAsync<DomainException>(() => _host.Sender.Send(
            new DispensePrescriptionForMedicationOrderCommand(prescription.Id),
            CancellationToken.None));

        Assert.Equal(PrescriptionErrorCodes.PrescriptionExpired, ex.Code);
    }

    [Fact]
    public async Task Dispense_one_minute_before_expiry_succeeds()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var patient = await SeedPatientAsync();
        var issuedAtUtc = _timeProvider.GetUtcNow().UtcDateTime
            .AddDays(-PrescriptionPolicies.DefaultExpiryDays)
            .AddMinutes(1);
        var prescription = await IssuePrescriptionAsync(doctor, patient, issuedAtUtc);

        _host.CurrentUser.UserId = patient.UserId;
        var result = await _host.Sender.Send(
            new DispensePrescriptionForMedicationOrderCommand(prescription.Id),
            CancellationToken.None);

        Assert.Equal("dispensed", result.Status);
    }

    [Fact]
    public async Task Explicit_short_expiry_causes_expired_dispense_after_window()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var patient = await SeedPatientAsync();
        var issuedAtUtc = _timeProvider.GetUtcNow().UtcDateTime.AddDays(-10);
        var expiresAtUtc = issuedAtUtc.AddDays(7);

        _host.CurrentUser.UserId = doctor.UserId;
        var restoreUtc = _timeProvider.GetUtcNow().UtcDateTime;
        _timeProvider.SetUtcNow(issuedAtUtc);
        PrescriptionDto prescription;
        try
        {
            prescription = await _host.Sender.Send(
                new CreatePrescriptionCommand(
                    patient.Id,
                    "Amoxicillin",
                    "500mg",
                    "Twice daily",
                    7,
                    null,
                    expiresAtUtc,
                    null),
                CancellationToken.None);
        }
        finally
        {
            _timeProvider.SetUtcNow(restoreUtc);
        }

        _host.CurrentUser.UserId = patient.UserId;
        var ex = await Assert.ThrowsAsync<DomainException>(() => _host.Sender.Send(
            new DispensePrescriptionForMedicationOrderCommand(prescription.Id),
            CancellationToken.None));

        Assert.Equal(PrescriptionErrorCodes.PrescriptionExpired, ex.Code);
    }

    [Fact]
    public async Task Dispense_unknown_prescription_returns_prescription_required()
    {
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var ex = await Assert.ThrowsAsync<DomainException>(() => _host.Sender.Send(
            new DispensePrescriptionForMedicationOrderCommand(Guid.CreateVersion7()),
            CancellationToken.None));

        Assert.Equal(PrescriptionErrorCodes.PrescriptionRequired, ex.Code);
    }

    [Fact]
    public async Task Cancel_rejects_whitespace_only_reason()
    {
        var validator = new CancelPrescriptionCommandValidator();
        var result = validator.TestValidate(new CancelPrescriptionCommand(Guid.CreateVersion7(), "   "));
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public async Task Cancelled_prescription_cannot_be_dispensed()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var patient = await SeedPatientAsync();
        var prescription = await IssuePrescriptionAsync(doctor, patient, _timeProvider.GetUtcNow().UtcDateTime);

        _host.CurrentUser.UserId = doctor.UserId;
        await _host.Sender.Send(
            new CancelPrescriptionCommand(prescription.Id, "No longer required"),
            CancellationToken.None);

        _host.CurrentUser.UserId = patient.UserId;
        var ex = await Assert.ThrowsAsync<DomainException>(() => _host.Sender.Send(
            new DispensePrescriptionForMedicationOrderCommand(prescription.Id),
            CancellationToken.None));

        Assert.Equal(PrescriptionErrorCodes.PrescriptionRequired, ex.Code);
    }

    [Fact]
    public void Drug_interaction_checker_returns_empty_for_empty_active_schedule()
    {
        var checker = new StaticDrugInteractionChecker();
        var alerts = checker.Check("Ibuprofen", []);
        Assert.Empty(alerts);
    }

    [Fact]
    public async Task Create_prescription_with_empty_schedule_writes_no_interaction_outbox_events()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var patient = await SeedPatientAsync();

        _host.CurrentUser.UserId = doctor.UserId;
        await _host.Sender.Send(
            new CreatePrescriptionCommand(
                patient.Id,
                "Ibuprofen",
                "400mg",
                "Every 8 hours",
                5,
                null,
                null,
                null),
            CancellationToken.None);

        Assert.False(await _host.DbContext.DomainEventOutbox
            .AnyAsync(x => x.EventType.Contains("DrugInteractionAlertDetectedDomainEvent")));
    }

    private async Task<Doctor> SeedVerifiedDoctorAsync()
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);
        return await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
    }

    private async Task<Patient> SeedPatientAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Edge Case Patient",
                null,
                $"patient-edge-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
    }

    private async Task<PrescriptionDto> IssuePrescriptionAsync(
        Doctor doctor,
        Patient patient,
        DateTime issuedAtUtc)
    {
        _host.CurrentUser.UserId = doctor.UserId;
        var restoreUtc = _timeProvider.GetUtcNow().UtcDateTime;
        _timeProvider.SetUtcNow(issuedAtUtc);

        try
        {
            return await _host.Sender.Send(
                new CreatePrescriptionCommand(
                    patient.Id,
                    "Amoxicillin",
                    "500mg",
                    "Twice daily",
                    7,
                    null,
                    null,
                    null),
                CancellationToken.None);
        }
        finally
        {
            _timeProvider.SetUtcNow(restoreUtc);
        }
    }
}
