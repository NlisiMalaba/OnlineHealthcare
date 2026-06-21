using System.Text.Json;
using FsCheck.Xunit;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RejectDoctorLicense;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Identity.Events;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Properties;

public sealed class LicenseVerificationPropertyTests
{
    private const string VerifiedEventType =
        "HealthPlatform.Domain.Identity.Events.DoctorLicenseVerifiedDomainEvent";

    private const string RejectedEventType =
        "HealthPlatform.Domain.Identity.Events.DoctorLicenseRejectedDomainEvent";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    // Feature: online-healthcare-platform, Property 5: License Verification State Transition
    [Property(
        Arbitrary = [typeof(LicenseVerificationArbitraries), typeof(DoctorRegistrationArbitraries)],
        MaxTest = 100)]
    public bool Pending_doctor_license_verification_transitions_with_notification(
        LicenseVerificationTransitionCase transitionCase) =>
        RunLicenseVerificationInvariantAsync(transitionCase).GetAwaiter().GetResult();

    private static async Task<bool> RunLicenseVerificationInvariantAsync(
        LicenseVerificationTransitionCase transitionCase)
    {
        await using var host = new PatientRegistrationTestHost();

        var registration = await host.Sender.Send(
            transitionCase.Registration.ToCommand(),
            CancellationToken.None);

        if (!string.Equals(registration.VerificationStatus, "pending", StringComparison.Ordinal))
        {
            return false;
        }

        LicenseVerificationResultDto result;
        if (transitionCase.VerifyLicense)
        {
            result = await host.Sender.Send(
                new VerifyDoctorLicenseCommand(registration.DoctorId),
                CancellationToken.None);
        }
        else
        {
            result = await host.Sender.Send(
                new RejectDoctorLicenseCommand(registration.DoctorId, transitionCase.RejectionReason),
                CancellationToken.None);
        }

        var doctor = await host.DbContext.Doctors
            .AsNoTracking()
            .SingleOrDefaultAsync(d => d.Id == registration.DoctorId);

        if (doctor is null)
        {
            return false;
        }

        if (transitionCase.VerifyLicense)
        {
            return doctor.VerificationStatus == DoctorVerificationStatus.Verified
                && string.Equals(result.VerificationStatus, "verified", StringComparison.Ordinal)
                && result.RejectionReason is null
                && doctor.RejectionReason is null
                && await HasVerifiedNotificationEventAsync(host, registration.DoctorId);
        }

        return doctor.VerificationStatus == DoctorVerificationStatus.Rejected
            && string.Equals(result.VerificationStatus, "rejected", StringComparison.Ordinal)
            && result.RejectionReason == transitionCase.RejectionReason
            && doctor.RejectionReason == transitionCase.RejectionReason
            && await HasRejectedNotificationEventAsync(
                host,
                registration.DoctorId,
                transitionCase.RejectionReason);
    }

    private static async Task<bool> HasVerifiedNotificationEventAsync(
        PatientRegistrationTestHost host,
        Guid doctorId)
    {
        var entries = await host.DbContext.DomainEventOutbox
            .AsNoTracking()
            .Where(x => x.EventType == VerifiedEventType)
            .ToListAsync();

        return entries
            .Select(DeserializeVerifiedEvent)
            .Any(e => e?.DoctorId == doctorId);
    }

    private static async Task<bool> HasRejectedNotificationEventAsync(
        PatientRegistrationTestHost host,
        Guid doctorId,
        string reason)
    {
        var entries = await host.DbContext.DomainEventOutbox
            .AsNoTracking()
            .Where(x => x.EventType == RejectedEventType)
            .ToListAsync();

        return entries
            .Select(DeserializeRejectedEvent)
            .Any(e => e?.DoctorId == doctorId && e.Reason == reason);
    }

    private static DoctorLicenseVerifiedDomainEvent? DeserializeVerifiedEvent(
        Infrastructure.Persistence.Entities.DomainEventOutboxEntry entry) =>
        JsonSerializer.Deserialize<DoctorLicenseVerifiedDomainEvent>(entry.Payload, SerializerOptions);

    private static DoctorLicenseRejectedDomainEvent? DeserializeRejectedEvent(
        Infrastructure.Persistence.Entities.DomainEventOutboxEntry entry) =>
        JsonSerializer.Deserialize<DoctorLicenseRejectedDomainEvent>(entry.Payload, SerializerOptions);
}
