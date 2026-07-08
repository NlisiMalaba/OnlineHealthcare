using FluentValidation;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Referrals.CompleteReferral;
using HealthPlatform.Application.Referrals.CreateReferral;
using HealthPlatform.Application.Referrals.RespondToReferral;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Referrals;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Referrals;

public sealed class ReferralWorkflowCoverageTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Referral_status_transitions_follow_expected_workflow()
    {
        var context = await SeedPendingReferralAsync();
        _host.CurrentUser.UserId = context.ReceivingDoctor.UserId;

        var requestedInfo = await _host.Sender.Send(
            new RespondToReferralCommand(
                context.Referral.Id,
                ReferralResponseAction.RequestAdditionalInformation,
                "Please include latest imaging."),
            CancellationToken.None);
        Assert.Equal("needsadditionalinformation", requestedInfo.Status);

        var accepted = await _host.Sender.Send(
            new RespondToReferralCommand(
                context.Referral.Id,
                ReferralResponseAction.Accept,
                null),
            CancellationToken.None);
        Assert.Equal("accepted", accepted.Status);

        var completed = await _host.Sender.Send(
            new CompleteReferralCommand(
                context.Referral.Id,
                "Specialist assessment complete; continue treatment plan."),
            CancellationToken.None);
        Assert.Equal("completed", completed.Status);
    }

    [Fact]
    public async Task Referral_completion_revokes_receiving_doctor_access_grant()
    {
        var context = await SeedPendingReferralAsync();
        _host.CurrentUser.UserId = context.ReceivingDoctor.UserId;

        await _host.Sender.Send(
            new RespondToReferralCommand(context.Referral.Id, ReferralResponseAction.Accept, null),
            CancellationToken.None);

        await _host.Sender.Send(
            new CompleteReferralCommand(context.Referral.Id, "Completed and summarized."),
            CancellationToken.None);

        var grant = await _host.DbContext.ReferralHealthRecordAccessGrants
            .SingleAsync(g => g.ReferralId == context.Referral.Id);
        Assert.NotNull(grant.RevokedAtUtc);
    }

    [Fact]
    public async Task Referral_creation_requires_patient_consent_timestamp()
    {
        var referringDoctor = await SeedVerifiedDoctorAsync("consent-referring");
        var receivingDoctor = await SeedVerifiedDoctorAsync("consent-receiving");
        var patient = await SeedPatientAsync("consent");
        _host.CurrentUser.UserId = referringDoctor.UserId;

        await Assert.ThrowsAsync<ValidationException>(() => _host.Sender.Send(
            new CreateReferralCommand(
                patient.Id,
                receivingDoctor.Id,
                null,
                "Referral without consent timestamp",
                null,
                ["diagnoses"],
                default),
            CancellationToken.None));
    }

    private async Task<(Referral Referral, Doctor ReceivingDoctor)> SeedPendingReferralAsync()
    {
        var referringDoctor = await SeedVerifiedDoctorAsync("workflow-referring");
        var receivingDoctor = await SeedVerifiedDoctorAsync("workflow-receiving");
        var patient = await SeedPatientAsync("workflow");
        _host.CurrentUser.UserId = referringDoctor.UserId;

        var created = await _host.Sender.Send(
            new CreateReferralCommand(
                patient.Id,
                receivingDoctor.Id,
                null,
                "Workflow transition validation referral",
                "Share relevant sections",
                ["diagnoses", "lab_results"],
                DateTime.UtcNow),
            CancellationToken.None);

        var referral = await _host.DbContext.Referrals.SingleAsync(r => r.Id == created.Id);
        return (referral, receivingDoctor);
    }

    private async Task<Doctor> SeedVerifiedDoctorAsync(string suffix)
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand($"doctor-{suffix}-{Guid.NewGuid():N}@example.com"),
            CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);
        return await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
    }

    private async Task<Patient> SeedPatientAsync(string suffix)
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
