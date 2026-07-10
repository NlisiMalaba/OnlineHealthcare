using HealthPlatform.Application.Referrals;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Referrals;

public sealed class LoggingReferralCreatedNotifier(
    ILogger<LoggingReferralCreatedNotifier> logger)
    : IReferralCreatedNotifier
{
    public Task NotifyReferralCreatedAsync(
        Guid patientUserId,
        Guid? receivingDoctorUserId,
        Guid referralId,
        Guid referringDoctorId,
        string reason,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Referral created notification requested for referral {ReferralId}, patient user {PatientUserId}, receiving doctor user {ReceivingDoctorUserId}, referring doctor {ReferringDoctorId}.",
            referralId,
            patientUserId,
            receivingDoctorUserId,
            referringDoctorId);
        return Task.CompletedTask;
    }
}
