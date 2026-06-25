using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Prescriptions;
using HealthPlatform.Application.Prescriptions.CancelPrescription;
using HealthPlatform.Application.Prescriptions.CreatePrescription;
using HealthPlatform.Application.Prescriptions.Dispensing;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Prescriptions;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Prescriptions;

public sealed class CancelPrescriptionCommandHandlerTests : IAsyncLifetime
{
    private CapturingPrescriptionCancelledNotifier _notifier = null!;
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _notifier = new CapturingPrescriptionCancelledNotifier();
        _host = new PatientRegistrationTestHost(prescriptionCancelledNotifier: _notifier);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Cancel_active_prescription_records_reason_and_notifies_patient()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var patient = await SeedPatientAsync();
        var issued = await IssuePrescriptionAsync(doctor, patient);

        _host.CurrentUser.UserId = doctor.UserId;
        var result = await _host.Sender.Send(
            new CancelPrescriptionCommand(issued.Id, "Patient no longer requires medication"),
            CancellationToken.None);

        Assert.Equal("cancelled", result.Status);
        Assert.Equal("Patient no longer requires medication", result.CancellationReason);

        var stored = await _host.DbContext.Prescriptions.SingleAsync(p => p.Id == issued.Id);
        Assert.Equal(PrescriptionStatus.Cancelled, stored.Status);

        Assert.Single(_notifier.Calls);
        Assert.Equal(patient.UserId, _notifier.Calls[0].PatientUserId);
        Assert.Equal(issued.Id, _notifier.Calls[0].PrescriptionId);
    }

    [Fact]
    public async Task Cancel_dispensed_prescription_returns_not_cancellable()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var patient = await SeedPatientAsync();
        var issued = await IssuePrescriptionAsync(doctor, patient);

        _host.CurrentUser.UserId = patient.UserId;
        await _host.Sender.Send(
            new DispensePrescriptionForMedicationOrderCommand(issued.Id),
            CancellationToken.None);

        _host.CurrentUser.UserId = doctor.UserId;
        var ex = await Assert.ThrowsAsync<DomainException>(() => _host.Sender.Send(
            new CancelPrescriptionCommand(issued.Id, "Too late"),
            CancellationToken.None));

        Assert.Equal(PrescriptionErrorCodes.PrescriptionNotCancellable, ex.Code);
    }

    [Fact]
    public async Task Cancel_is_idempotent_when_already_cancelled()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var patient = await SeedPatientAsync();
        var issued = await IssuePrescriptionAsync(doctor, patient);

        _host.CurrentUser.UserId = doctor.UserId;
        await _host.Sender.Send(
            new CancelPrescriptionCommand(issued.Id, "First reason"),
            CancellationToken.None);

        var result = await _host.Sender.Send(
            new CancelPrescriptionCommand(issued.Id, "Second reason"),
            CancellationToken.None);

        Assert.Equal("cancelled", result.Status);
        Assert.Equal("First reason", result.CancellationReason);
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
                "Patient One",
                null,
                $"patient-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
    }

    private async Task<PrescriptionDto> IssuePrescriptionAsync(Doctor doctor, Patient patient)
    {
        _host.CurrentUser.UserId = doctor.UserId;
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
}
