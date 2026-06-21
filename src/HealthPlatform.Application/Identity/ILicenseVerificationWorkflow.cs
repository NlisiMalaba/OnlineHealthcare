namespace HealthPlatform.Application.Identity;

public interface ILicenseVerificationWorkflow
{
    Task<LicenseVerificationResultDto> VerifyAsync(Guid doctorId, CancellationToken ct);

    Task<LicenseVerificationResultDto> RejectAsync(Guid doctorId, string reason, CancellationToken ct);
}
