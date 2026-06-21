namespace HealthPlatform.Application.Identity.RegisterDoctor;

public sealed record DoctorRegistrationResponseDto(
    Guid DoctorId,
    string VerificationStatus,
    DateTime CreatedAtUtc);
