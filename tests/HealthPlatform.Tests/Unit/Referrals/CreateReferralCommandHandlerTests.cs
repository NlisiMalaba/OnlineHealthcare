using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Referrals.CreateReferral;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Referrals;

public sealed class CreateReferralCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Create_referral_records_consent_and_notifies_patient_and_receiving_doctor()
    {
        var referringDoctor = await SeedVerifiedDoctorAsync("referring");
        var receivingDoctor = await SeedVerifiedDoctorAsync("receiving");
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = referringDoctor.UserId;
        var consentAtUtc = DateTime.UtcNow;

        var result = await _host.Sender.Send(
            new CreateReferralCommand(
                patient.Id,
                receivingDoctor.Id,
                null,
                "Requires specialist cardiology review",
                "Attach ECG notes and recent medication history.",
                ["diagnoses", "lab_results", "medications"],
                consentAtUtc),
            CancellationToken.None);

        Assert.Equal(referringDoctor.Id, result.ReferringDoctorId);
        Assert.Equal(receivingDoctor.Id, result.ReceivingDoctorId);
        Assert.Equal(consentAtUtc, result.PatientConsentAtUtc);
        Assert.Equal("pending", result.Status);
        Assert.Equal(3, result.SharedHealthRecordSections.Count);

        Assert.Single(_host.ReferralCreatedNotifier.Calls);
        var notification = _host.ReferralCreatedNotifier.Calls[0];
        Assert.Equal(patient.UserId, notification.PatientUserId);
        Assert.Equal(receivingDoctor.UserId, notification.ReceivingDoctorUserId);
        Assert.Equal(result.Id, notification.ReferralId);

        var stored = await _host.DbContext.Referrals.SingleAsync(r => r.Id == result.Id);
        Assert.Equal(patient.Id, stored.PatientId);
    }

    [Fact]
    public async Task Create_referral_for_hospital_notifies_patient_without_receiving_doctor()
    {
        var referringDoctor = await SeedVerifiedDoctorAsync("hospital-referring");
        var patient = await SeedPatientAsync("hospital");
        _host.CurrentUser.UserId = referringDoctor.UserId;

        var result = await _host.Sender.Send(
            new CreateReferralCommand(
                patient.Id,
                null,
                "Parirenyatwa Hospital",
                "Escalate for inpatient management",
                null,
                ["consultation_notes"],
                DateTime.UtcNow),
            CancellationToken.None);

        Assert.Null(result.ReceivingDoctorId);
        Assert.Equal("Parirenyatwa Hospital", result.ReceivingHospitalName);
        Assert.Single(_host.ReferralCreatedNotifier.Calls);
        Assert.Null(_host.ReferralCreatedNotifier.Calls[0].ReceivingDoctorUserId);
    }

    [Fact]
    public async Task Create_referral_rejects_unverified_doctor()
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);
        var doctor = await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
        var patient = await SeedPatientAsync("unverified");
        _host.CurrentUser.UserId = doctor.UserId;

        var ex = await Assert.ThrowsAsync<DomainException>(() => _host.Sender.Send(
            new CreateReferralCommand(
                patient.Id,
                null,
                "Referral Hospital",
                "Need specialist review",
                null,
                ["diagnoses"],
                DateTime.UtcNow),
            CancellationToken.None));

        Assert.Equal("DOCTOR_NOT_VERIFIED", ex.Code);
    }

    private async Task<Doctor> SeedVerifiedDoctorAsync(string suffix)
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand($"doctor-{suffix}-{Guid.NewGuid():N}@example.com"),
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

        return await _host.DbContext.Patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstAsync();
    }
}
