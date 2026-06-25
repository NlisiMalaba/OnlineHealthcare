using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Telemedicine.RecordingConsent;

public sealed record GrantRecordingConsentCommand(Guid AppointmentId) : ICommand<GrantRecordingConsentDto>;
