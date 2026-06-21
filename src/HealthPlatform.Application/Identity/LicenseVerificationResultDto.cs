namespace HealthPlatform.Application.Identity;

public sealed record LicenseVerificationResultDto(
    Guid DoctorId,
    string VerificationStatus,
    string? RejectionReason,
    DateTime UpdatedAtUtc);
