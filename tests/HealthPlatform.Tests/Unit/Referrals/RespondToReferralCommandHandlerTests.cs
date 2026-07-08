using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Referrals.CreateReferral;
using HealthPlatform.Application.Referrals.RespondToReferral;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Referrals;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Referrals;

public sealed class RespondToReferralCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Accept_grants_shared_access_and_notifies_referring_doctor_and_patient()
    {
        var (referral, referringDoctor, receivingDoctor, patient) = await SeedReferralAsync();
        _host.CurrentUser.UserId = receivingDoctor.UserId;

        var result = await _host.Sender.Send(
            new RespondToReferralCommand(referral.Id, ReferralResponseAction.Accept, null),
            CancellationToken.None);

        Assert.Equal("accepted", result.Status);
        Assert.NotNull(result.RespondedAtUtc);

        var stored = await _host.DbContext.Referrals.SingleAsync(r => r.Id == referral.Id);
        Assert.Equal(ReferralStatus.Accepted, stored.Status);

        var grant = await _host.DbContext.ReferralHealthRecordAccessGrants.SingleAsync(g => g.ReferralId == referral.Id);
        Assert.Equal(patient.Id, grant.PatientId);
        Assert.Equal(receivingDoctor.Id, grant.DoctorId);
        Assert.Equal(referral.SharedHealthRecordSections.Count, grant.SharedHealthRecordSections.Count);

        Assert.Single(_host.ReferralStatusChangedNotifier.Calls);
        var notification = _host.ReferralStatusChangedNotifier.Calls[0];
        Assert.Equal(patient.UserId, notification.PatientUserId);
        Assert.Equal(referringDoctor.UserId, notification.ReferringDoctorUserId);
        Assert.Equal("accepted", notification.Status);
    }

    [Fact]
    public async Task Decline_requires_receiving_doctor_and_persists_reason()
    {
        var (referral, _, _, _) = await SeedReferralAsync();
        var outsiderDoctor = await SeedVerifiedDoctorAsync("outsider");
        _host.CurrentUser.UserId = outsiderDoctor.UserId;

        var ex = await Assert.ThrowsAsync<AccessDeniedException>(() => _host.Sender.Send(
            new RespondToReferralCommand(referral.Id, ReferralResponseAction.Decline, "No availability"),
            CancellationToken.None));

        Assert.Equal("REFERRAL_ACCESS_DENIED", ex.Code);
    }

    [Fact]
    public async Task Request_additional_information_updates_status_and_notifies()
    {
        var (referral, referringDoctor, receivingDoctor, patient) = await SeedReferralAsync();
        _host.CurrentUser.UserId = receivingDoctor.UserId;
        const string message = "Please attach latest blood panel and imaging report.";

        var result = await _host.Sender.Send(
            new RespondToReferralCommand(
                referral.Id,
                ReferralResponseAction.RequestAdditionalInformation,
                message),
            CancellationToken.None);

        Assert.Equal("needsadditionalinformation", result.Status);
        Assert.Equal(message, result.ResponseReason);
        Assert.Single(_host.ReferralStatusChangedNotifier.Calls);
        var notification = _host.ReferralStatusChangedNotifier.Calls[0];
        Assert.Equal(patient.UserId, notification.PatientUserId);
        Assert.Equal(referringDoctor.UserId, notification.ReferringDoctorUserId);
        Assert.Equal(message, notification.Reason);
    }

    private async Task<(Referral Referral, Doctor ReferringDoctor, Doctor ReceivingDoctor, Patient Patient)> SeedReferralAsync()
    {
        var referringDoctor = await SeedVerifiedDoctorAsync("referring");
        var receivingDoctor = await SeedVerifiedDoctorAsync("receiving");
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = referringDoctor.UserId;

        var referralResult = await _host.Sender.Send(
            new CreateReferralCommand(
                patient.Id,
                receivingDoctor.Id,
                null,
                "Complex neurology review required",
                "Include prior consultation notes.",
                ["diagnoses", "lab_results", "medications"],
                DateTime.UtcNow),
            CancellationToken.None);

        var referral = await _host.DbContext.Referrals.SingleAsync(r => r.Id == referralResult.Id);
        _host.ReferralStatusChangedNotifier.Calls.Clear();
        return (referral, referringDoctor, receivingDoctor, patient);
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
