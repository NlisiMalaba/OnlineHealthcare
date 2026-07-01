using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Prescriptions;
using HealthPlatform.Application.Prescriptions.CreatePrescription;
using HealthPlatform.Application.Prescriptions.Dispensing;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Prescriptions;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Prescriptions;

public sealed class DispensePrescriptionForMedicationOrderCommandHandlerTests : IAsyncLifetime
{
    private FakeTimeProvider _timeProvider = null!;
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _timeProvider = new FakeTimeProvider(new DateTime(2026, 6, 24, 12, 0, 0, DateTimeKind.Utc));
        _host = new PatientRegistrationTestHost(timeProvider: _timeProvider);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task First_order_marks_prescription_as_dispensed()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var patient = await SeedPatientAsync();
        var prescription = await IssuePrescriptionAsync(doctor, patient);

        _host.CurrentUser.UserId = patient.UserId;
        var result = await _host.Sender.Send(
            new DispensePrescriptionForMedicationOrderCommand(prescription.Id),
            CancellationToken.None);

        Assert.Equal("dispensed", result.Status);

        var stored = await _host.DbContext.Prescriptions.SingleAsync(p => p.Id == prescription.Id);
        Assert.Equal(PrescriptionStatus.Dispensed, stored.Status);

        var schedule = await _host.DbContext.MedicationSchedules
            .SingleAsync(s => s.PrescriptionId == prescription.Id);
        Assert.Equal("Amoxicillin", schedule.MedicationName);
        Assert.Equal(14, schedule.DoseTimes.Count);
    }

    [Fact]
    public async Task Second_order_attempt_returns_prescription_dispensed()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var patient = await SeedPatientAsync();
        var prescription = await IssuePrescriptionAsync(doctor, patient);

        _host.CurrentUser.UserId = patient.UserId;
        await _host.Sender.Send(
            new DispensePrescriptionForMedicationOrderCommand(prescription.Id),
            CancellationToken.None);

        var ex = await Assert.ThrowsAsync<DomainException>(() => _host.Sender.Send(
            new DispensePrescriptionForMedicationOrderCommand(prescription.Id),
            CancellationToken.None));

        Assert.Equal(PrescriptionErrorCodes.PrescriptionDispensed, ex.Code);
    }

    [Fact]
    public async Task Expired_prescription_returns_prescription_expired()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var patient = await SeedPatientAsync();
        var prescription = await IssuePrescriptionAsync(
            doctor,
            patient,
            issuedAtUtc: _timeProvider.GetUtcNow().UtcDateTime.AddDays(-40));

        _host.CurrentUser.UserId = patient.UserId;
        var ex = await Assert.ThrowsAsync<DomainException>(() => _host.Sender.Send(
            new DispensePrescriptionForMedicationOrderCommand(prescription.Id),
            CancellationToken.None));

        Assert.Equal(PrescriptionErrorCodes.PrescriptionExpired, ex.Code);
    }

    [Fact]
    public async Task Prescription_for_another_patient_returns_prescription_required()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var patientOne = await SeedPatientAsync("one");
        var patientTwo = await SeedPatientAsync("two");
        var prescription = await IssuePrescriptionAsync(doctor, patientOne);

        _host.CurrentUser.UserId = patientTwo.UserId;
        var ex = await Assert.ThrowsAsync<DomainException>(() => _host.Sender.Send(
            new DispensePrescriptionForMedicationOrderCommand(prescription.Id),
            CancellationToken.None));

        Assert.Equal(PrescriptionErrorCodes.PrescriptionRequired, ex.Code);
    }

    private async Task<Doctor> SeedVerifiedDoctorAsync()
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);
        return await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
    }

    private async Task<Patient> SeedPatientAsync(string suffix = "default")
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                $"Patient {suffix}",
                null,
                $"patient-{suffix}-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
    }

    private async Task<PrescriptionDto> IssuePrescriptionAsync(
        Doctor doctor,
        Patient patient,
        DateTime? issuedAtUtc = null)
    {
        _host.CurrentUser.UserId = doctor.UserId;

        var restoreUtc = _timeProvider.GetUtcNow().UtcDateTime;
        if (issuedAtUtc.HasValue)
        {
            _timeProvider.SetUtcNow(issuedAtUtc.Value);
        }

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
