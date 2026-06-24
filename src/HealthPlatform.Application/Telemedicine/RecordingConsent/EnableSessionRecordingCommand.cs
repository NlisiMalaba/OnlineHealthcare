using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Telemedicine.RecordingConsent;

public sealed record EnableSessionRecordingCommand(Guid AppointmentId) : ICommand<EnableSessionRecordingDto>;
