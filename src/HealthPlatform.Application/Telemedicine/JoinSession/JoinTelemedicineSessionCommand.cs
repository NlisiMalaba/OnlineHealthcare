using HealthPlatform.Application.Behaviors;
using HealthPlatform.Domain.Telemedicine;

namespace HealthPlatform.Application.Telemedicine.JoinSession;

public sealed record JoinTelemedicineSessionCommand(
    Guid AppointmentId,
    TelemedicineSessionMode? Mode) : ICommand<JoinTelemedicineSessionDto>;
