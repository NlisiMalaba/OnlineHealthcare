namespace HealthPlatform.Domain.Identity;

public sealed class InvalidDoctorVerificationStatusException(DoctorVerificationStatus currentStatus)
    : Exception($"Doctor verification cannot transition from '{currentStatus}'.")
{
    public DoctorVerificationStatus CurrentStatus { get; } = currentStatus;
}
