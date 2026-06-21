namespace HealthPlatform.Application.Identity.RegisterPatient;

public sealed record PatientRegistrationResponseDto(
    Guid PatientId,
    Guid HealthRecordId,
    DateTime CreatedAtUtc);
