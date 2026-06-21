using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Identity.VerifyDoctorLicense;

public sealed record VerifyDoctorLicenseCommand(Guid DoctorId) : ICommand<LicenseVerificationResultDto>;
