using FsCheck;
using FsCheck.Xunit;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Referrals;
using HealthPlatform.Application.Referrals.CreateReferral;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Infrastructure.Referrals;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace HealthPlatform.Tests.Properties;

public sealed class ReferralTimeoutReminderPropertyTests
{
    // Feature: online-healthcare-platform, Property 36: Referral Timeout Reminder
    [Property(MaxTest = 100)]
    public bool Pending_referral_older_than_48_hours_emits_timeout_reminder(PositiveInt rawMinutesAgo) =>
        RunTimeoutReminderInvariantAsync(rawMinutesAgo.Get).GetAwaiter().GetResult();

    private static async Task<bool> RunTimeoutReminderInvariantAsync(int rawMinutesAgo)
    {
        var minutesAgo = Math.Clamp(rawMinutesAgo, 1, 7 * 24 * 60);
        var now = new DateTime(2026, 7, 8, 8, 0, 0, DateTimeKind.Utc);
        var clock = new FakeTimeProvider(now);
        await using var host = new PatientRegistrationTestHost(timeProvider: clock);

        var referringDoctor = await SeedVerifiedDoctorAsync(host, "referring");
        var receivingDoctor = await SeedVerifiedDoctorAsync(host, "receiving");
        var patient = await SeedPatientAsync(host);
        host.CurrentUser.UserId = referringDoctor.UserId;

        var created = await host.Sender.Send(
            new CreateReferralCommand(
                patient.Id,
                receivingDoctor.Id,
                null,
                "Timeout reminder property test referral",
                "Share referral context",
                ["diagnoses"],
                now.AddMinutes(-5)),
            CancellationToken.None);

        var createdAtUtc = now.AddMinutes(-minutesAgo);
        var referral = await host.DbContext.Referrals.SingleAsync(r => r.Id == created.Id);
        host.DbContext.Entry(referral).Property("CreatedAtUtc").CurrentValue = createdAtUtc;
        host.DbContext.Entry(referral).Property("UpdatedAtUtc").CurrentValue = createdAtUtc;
        await host.DbContext.SaveChangesAsync();
        host.DbContext.ChangeTracker.Clear();

        var dispatcher = new ReferralTimeoutReminderDispatcher(
            clock,
            host.GetRequiredService<IReferralRepository>(),
            host.GetRequiredService<HealthPlatform.Application.Identity.IDoctorRepository>(),
            host.ReferralTimeoutReminderNotifier,
            NullLogger<ReferralTimeoutReminderDispatcher>.Instance);

        var firstRun = await dispatcher.DispatchDueRemindersAsync(CancellationToken.None);
        var secondRun = await dispatcher.DispatchDueRemindersAsync(CancellationToken.None);
        var expectedReminder = minutesAgo >= (int)ReferralPolicies.TimeoutReminderThreshold.TotalMinutes;

        if (expectedReminder)
        {
            if (firstRun != 1 || secondRun != 0 || host.ReferralTimeoutReminderNotifier.Calls.Count != 1)
            {
                return false;
            }
        }
        else if (firstRun != 0 || secondRun != 0 || host.ReferralTimeoutReminderNotifier.Calls.Count != 0)
        {
            return false;
        }

        var storedReferral = await host.DbContext.Referrals.SingleAsync(r => r.Id == created.Id);
        return expectedReminder == storedReferral.TimeoutReminderSentAtUtc.HasValue;
    }

    private static async Task<Doctor> SeedVerifiedDoctorAsync(PatientRegistrationTestHost host, string suffix)
    {
        var registration = await host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand($"doctor-{suffix}-{Guid.NewGuid():N}@example.com"),
            CancellationToken.None);
        await host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);
        return await host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
    }

    private static async Task<Patient> SeedPatientAsync(PatientRegistrationTestHost host)
    {
        await host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Property Test Patient",
                null,
                $"property-patient-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
    }
}
