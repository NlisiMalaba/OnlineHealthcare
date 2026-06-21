using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Identity.RejectDoctorLicense;

public sealed record RejectDoctorLicenseCommand(Guid DoctorId, string Reason)
    : ICommand<LicenseVerificationResultDto>;
