using FsCheck;
using FsCheck.Xunit;
using HealthPlatform.Application.Referrals.CreateReferral;

namespace HealthPlatform.Tests.Properties;

public sealed class ReferralRequiresPatientConsentPropertyTests
{
    // Feature: online-healthcare-platform, Property 39: Referral Requires Patient Consent
    [Property(MaxTest = 500)]
    public Property Referral_creation_requires_explicit_patient_consent_timestamp(int minuteOffset, bool hasConsent)
    {
        var validator = new CreateReferralCommandValidator();
        var consentAtUtc = hasConsent
            ? DateTime.SpecifyKind(DateTime.UnixEpoch.AddMinutes(minuteOffset), DateTimeKind.Utc)
            : default;

        var command = new CreateReferralCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            null,
            "Escalate care to specialist",
            "Include recent consultation findings.",
            ["diagnoses", "medications"],
            consentAtUtc);

        var result = validator.Validate(command);
        return (hasConsent == result.IsValid).ToProperty();
    }
}
