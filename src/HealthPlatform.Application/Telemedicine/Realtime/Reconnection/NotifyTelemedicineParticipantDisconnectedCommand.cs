using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Telemedicine.Realtime.Reconnection;

public sealed record NotifyTelemedicineParticipantDisconnectedCommand(Guid AppointmentId) : ICommand;
