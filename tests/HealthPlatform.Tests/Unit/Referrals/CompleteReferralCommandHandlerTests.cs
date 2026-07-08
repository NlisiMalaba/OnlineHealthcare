using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Referrals.CompleteReferral;
using HealthPlatform.Application.Referrals.CreateReferral;
using HealthPlatform.Application.Referrals.RespondToReferral;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Referrals;
using HealthPlatform.Infrastructure.MongoDb;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Referrals;

public sealed class CompleteReferralCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Completion_attaches_summary_revokes_access_and_sets_completed_status()
    {
        var context = await SeedAcceptedReferralAsync();
        _host.CurrentUser.UserId = context.ReceivingDoctor.UserId;

        var result = await _host.Sender.Send(
            new CompleteReferralCommand(context.Referral.Id, "Neurology consultation complete."),
            CancellationToken.None);

        Assert.Equal("completed", result.Status);
        Assert.NotNull(result.ConsultationSummaryEntryId);
        Assert.NotNull(result.RespondedAtUtc);

        var accessGrant = await _host.DbContext.ReferralHealthRecordAccessGrants
            .SingleAsync(g => g.ReferralId == context.Referral.Id);
        Assert.NotNull(accessGrant.RevokedAtUtc);

        var summary = _host.GetRequiredService<InMemoryHealthRecordEntryRepository>()
            .ReferralSummaries
            .Single(s => s.ReferralId == context.Referral.Id);
        Assert.Equal(context.Patient.Id, summary.PatientId);
        Assert.Equal(context.ReceivingDoctor.Id, summary.DoctorId);

        Assert.Single(_host.ReferralStatusChangedNotifier.Calls);
        Assert.Equal("completed", _host.ReferralStatusChangedNotifier.Calls[0].Status);
    }

    [Fact]
    public async Task Completion_requires_receiving_doctor()
    {
        var context = await SeedAcceptedReferralAsync();
        var outsider = await SeedVerifiedDoctorAsync("outsider");
        _host.CurrentUser.UserId = outsider.UserId;

        var ex = await Assert.ThrowsAsync<AccessDeniedException>(() => _host.Sender.Send(
            new CompleteReferralCommand(context.Referral.Id, "Complete."),
            CancellationToken.None));

        Assert.Equal("REFERRAL_ACCESS_DENIED", ex.Code);
    }

    [Fact]
    public async Task Completion_requires_accepted_referral()
    {
        var context = await SeedPendingReferralAsync();
        _host.CurrentUser.UserId = context.ReceivingDoctor.UserId;

        var ex = await Assert.ThrowsAsync<DomainException>(() => _host.Sender.Send(
            new CompleteReferralCommand(context.Referral.Id, "Complete."),
            CancellationToken.None));

        Assert.Equal("REFERRAL_COMPLETION_NOT_ALLOWED", ex.Code);
    }

    private async Task<(Referral Referral, Doctor ReferringDoctor, Doctor ReceivingDoctor, Patient Patient)> SeedAcceptedReferralAsync()
    {
        var context = await SeedPendingReferralAsync();
        _host.CurrentUser.UserId = context.ReceivingDoctor.UserId;
        await _host.Sender.Send(
            new RespondToReferralCommand(context.Referral.Id, ReferralResponseAction.Accept, null),
            CancellationToken.None);

        var acceptedReferral = await _host.DbContext.Referrals.SingleAsync(r => r.Id == context.Referral.Id);
        _host.ReferralStatusChangedNotifier.Calls.Clear();
        return (acceptedReferral, context.ReferringDoctor, context.ReceivingDoctor, context.Patient);
    }

    private async Task<(Referral Referral, Doctor ReferringDoctor, Doctor ReceivingDoctor, Patient Patient)> SeedPendingReferralAsync()
    {
        var referringDoctor = await SeedVerifiedDoctorAsync("referring");
        var receivingDoctor = await SeedVerifiedDoctorAsync("receiving");
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = referringDoctor.UserId;

        var created = await _host.Sender.Send(
            new CreateReferralCommand(
                patient.Id,
                receivingDoctor.Id,
                null,
                "Neurology escalation",
                "Share core records",
                ["diagnoses", "lab_results"],
                DateTime.UtcNow),
            CancellationToken.None);

        var referral = await _host.DbContext.Referrals.SingleAsync(r => r.Id == created.Id);
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
