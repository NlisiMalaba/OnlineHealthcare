namespace HealthPlatform.Application.Telemedicine.RecordingConsent;

public sealed record EnableSessionRecordingDto(
    Guid SessionId,
    Guid AppointmentId,
    bool RecordingConsent,
    bool RecordingEnabled);
