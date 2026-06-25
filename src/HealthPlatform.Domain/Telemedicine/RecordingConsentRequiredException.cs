namespace HealthPlatform.Domain.Telemedicine;

public sealed class RecordingConsentRequiredException()
    : Exception("Recording cannot be enabled without patient consent.");

public sealed class RecordingConsentNotAllowedException(TelemedicineSessionStatus status)
    : Exception($"Recording consent can only be granted before the session begins. Current status: {status}.")
{
    public TelemedicineSessionStatus Status { get; } = status;
}
