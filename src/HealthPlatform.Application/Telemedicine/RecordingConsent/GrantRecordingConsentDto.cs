namespace HealthPlatform.Application.Telemedicine.RecordingConsent;

public sealed record GrantRecordingConsentDto(
    Guid SessionId,
    Guid AppointmentId,
    bool RecordingConsent,
    bool RecordingEnabled);
