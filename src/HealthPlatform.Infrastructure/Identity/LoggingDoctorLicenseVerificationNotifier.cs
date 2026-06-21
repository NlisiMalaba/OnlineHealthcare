using HealthPlatform.Application.Identity;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Identity;

/// <summary>
/// Development-oriented stub for doctor license verification notifications; replace with email/SMS/push in production.
/// </summary>
public sealed class LoggingDoctorLicenseVerificationNotifier(
    ILogger<LoggingDoctorLicenseVerificationNotifier> logger)
    : IDoctorLicenseVerificationNotifier
{
    public Task NotifyLicenseVerifiedAsync(
        Guid userId,
        Guid doctorId,
        string fullName,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Doctor license verified notification requested for user {UserId}, doctor {DoctorId}.",
            userId,
            doctorId);
        return Task.CompletedTask;
    }

    public Task NotifyLicenseRejectedAsync(
        Guid userId,
        Guid doctorId,
        string fullName,
        string reason,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Doctor license rejected notification requested for user {UserId}, doctor {DoctorId} with reason code {ReasonCode}.",
            userId,
            doctorId,
            IdentityErrorCodes.LicenseInvalid);
        return Task.CompletedTask;
    }
}
