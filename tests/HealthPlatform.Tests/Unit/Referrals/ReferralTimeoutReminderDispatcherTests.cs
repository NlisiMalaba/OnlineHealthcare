using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Referrals;
using HealthPlatform.Application.Referrals.CreateReferral;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Infrastructure.Referrals;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HealthPlatform.Tests.Unit.Referrals;

public sealed class ReferralTimeoutReminderDispatcherTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;
    private FakeTimeProvider _clock = null!;

    public async Task InitializeAsync()
    {
        _clock = new FakeTimeProvider(new DateTime(2026, 7, 8, 8, 0, 0, DateTimeKind.Utc));
        _host = new PatientRegistrationTestHost(timeProvider: _clock);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task DispatchDueReminders_sends_timeout_notification_after_48_hours_and_marks_sent()
    {
        var (referralId, patient, receivingDoctor) = await SeedPendingReferralAsync();
        await BackdateReferralCreatedAtAsync(referralId, _clock.GetUtcNow().UtcDateTime - ReferralPolicies.TimeoutReminderThreshold - TimeSpan.FromMinutes(1));
        var dispatcher = CreateDispatcher();

        var dispatched = await dispatcher.DispatchDueRemindersAsync(CancellationToken.None);

        Assert.Equal(1, dispatched);
        Assert.Single(_host.ReferralTimeoutReminderNotifier.Calls);
        Assert.Equal(receivingDoctor.UserId, _host.ReferralTimeoutReminderNotifier.Calls[0].ReceivingDoctorUserId);
        Assert.Equal(patient.Id, _host.ReferralTimeoutReminderNotifier.Calls[0].PatientId);

        var referral = await _host.DbContext.Referrals.SingleAsync(r => r.Id == referralId);
        Assert.NotNull(referral.TimeoutReminderSentAtUtc);
    }

    [Fact]
    public async Task DispatchDueReminders_is_idempotent_after_reminder_sent()
    {
        var (referralId, _, _) = await SeedPendingReferralAsync();
        await BackdateReferralCreatedAtAsync(referralId, _clock.GetUtcNow().UtcDateTime - ReferralPolicies.TimeoutReminderThreshold - TimeSpan.FromMinutes(1));
        var dispatcher = CreateDispatcher();

        var firstRun = await dispatcher.DispatchDueRemindersAsync(CancellationToken.None);
        var secondRun = await dispatcher.DispatchDueRemindersAsync(CancellationToken.None);

        Assert.Equal(1, firstRun);
        Assert.Equal(0, secondRun);
        Assert.Single(_host.ReferralTimeoutReminderNotifier.Calls);
    }

    private ReferralTimeoutReminderDispatcher CreateDispatcher() =>
        new(
            _clock,
            _host.GetRequiredService<IReferralRepository>(),
            _host.GetRequiredService<HealthPlatform.Application.Identity.IDoctorRepository>(),
            _host.ReferralTimeoutReminderNotifier,
            NullLogger<ReferralTimeoutReminderDispatcher>.Instance);

    private async Task<(Guid ReferralId, Patient Patient, Doctor ReceivingDoctor)> SeedPendingReferralAsync()
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
                "Follow-up specialist referral",
                "Share triage history",
                ["diagnoses"],
                _clock.GetUtcNow().UtcDateTime.AddMinutes(-5)),
            CancellationToken.None);

        return (created.Id, patient, receivingDoctor);
    }

    private async Task BackdateReferralCreatedAtAsync(Guid referralId, DateTime createdAtUtc)
    {
        var referral = await _host.DbContext.Referrals.SingleAsync(r => r.Id == referralId);
        _host.DbContext.Entry(referral).Property("CreatedAtUtc").CurrentValue = createdAtUtc;
        _host.DbContext.Entry(referral).Property("UpdatedAtUtc").CurrentValue = createdAtUtc;
        await _host.DbContext.SaveChangesAsync();
        _host.DbContext.ChangeTracker.Clear();
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

        return await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
    }
}
