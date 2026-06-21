namespace HealthPlatform.Application.Identity;

public sealed record PharmacyProfileDto(
    Guid PharmacyId,
    string Name,
    string Address,
    double? Latitude,
    double? Longitude,
    string ContactEmail,
    string ContactPhone,
    string? LogoUrl,
    string VerificationStatus,
    DateTime UpdatedAtUtc);
