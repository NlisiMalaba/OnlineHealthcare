namespace HealthPlatform.Application.Identity.RegisterPharmacy;

public sealed record PharmacyRegistrationResponseDto(
    Guid PharmacyId,
    string VerificationStatus,
    DateTime CreatedAtUtc);
