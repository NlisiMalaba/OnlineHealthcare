using HealthPlatform.Application.Referrals;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Referrals;

public sealed class LoggingReferralStatusChangedNotifier(
    ILogger<LoggingReferralStatusChangedNotifier> logger)
    : IReferralStatusChangedNotifier
{
    public Task NotifyReferralStatusChangedAsync(
        Guid patientUserId,
        Guid referringDoctorUserId,
        Guid referralId,
        string status,
        string? reason,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Referral status change notification requested for referral {ReferralId}, status {ReferralStatus}, patient user {PatientUserId}, referring doctor user {ReferringDoctorUserId}.",
            referralId,
            status,
            patientUserId,
            referringDoctorUserId);
        return Task.CompletedTask;
    }
}
