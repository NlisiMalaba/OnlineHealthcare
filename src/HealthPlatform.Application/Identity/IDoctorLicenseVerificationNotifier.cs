namespace HealthPlatform.Application.Identity;

public interface IDoctorLicenseVerificationNotifier
{
    Task NotifyLicenseVerifiedAsync(Guid userId, Guid doctorId, string fullName, CancellationToken ct);

    Task NotifyLicenseRejectedAsync(
        Guid userId,
        Guid doctorId,
        string fullName,
        string reason,
        CancellationToken ct);
}
