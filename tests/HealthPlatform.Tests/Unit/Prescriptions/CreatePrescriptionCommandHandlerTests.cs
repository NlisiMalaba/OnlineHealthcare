using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Prescriptions;
using HealthPlatform.Application.Prescriptions.CreatePrescription;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Prescriptions;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Prescriptions;

public sealed class CreatePrescriptionCommandHandlerTests : IAsyncLifetime
{
    private CapturingPrescriptionIssuedNotifier _notifier = null!;
    private FakeTimeProvider _timeProvider = null!;
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _notifier = new CapturingPrescriptionIssuedNotifier();
        _timeProvider = new FakeTimeProvider(new DateTime(2026, 6, 24, 12, 0, 0, DateTimeKind.Utc));
        _host = new PatientRegistrationTestHost(prescriptionIssuedNotifier: _notifier, timeProvider: _timeProvider);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Create_prescription_links_doctor_patient_and_health_record()
    {
        var doctor = await SeedVerifiedDoctorAsync();
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = doctor.UserId;

        var result = await _host.Sender.Send(
            new CreatePrescriptionCommand(
                patient.Id,
                "Amoxicillin",
                "500mg",
                "Twice daily",
                7,
                "Take with food",
                null,
                null),
            CancellationToken.None);

        Assert.Equal(doctor.Id, result.DoctorId);
        Assert.Equal(patient.Id, result.PatientId);
        Assert.Equal("active", result.Status);
        Assert.Equal(_timeProvider.GetUtcNow().UtcDateTime.AddDays(PrescriptionPolicies.DefaultExpiryDays), result.ExpiresAtUtc);

        var stored = await _host.DbContext.Prescriptions.SingleAsync(p => p.Id == result.Id);
        Assert.Equal(result.HealthRecordId, stored.HealthRecordId);

        Assert.Single(_notifier.Calls);
        Assert.Equal(patient.UserId, _notifier.Calls[0].PatientUserId);
        Assert.Equal(result.Id, _notifier.Calls[0].PrescriptionId);
    }

    [Fact]
    public async Task Create_prescription_rejects_unverified_doctor()
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);
        var doctor = await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = doctor.UserId;

        var ex = await Assert.ThrowsAsync<DomainException>(() => _host.Sender.Send(
            new CreatePrescriptionCommand(
                patient.Id,
                "Amoxicillin",
                "500mg",
                "Twice daily",
                7,
                null,
                null,
                null),
            CancellationToken.None));

        Assert.Equal(PrescriptionErrorCodes.DoctorNotVerified, ex.Code);
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
}
