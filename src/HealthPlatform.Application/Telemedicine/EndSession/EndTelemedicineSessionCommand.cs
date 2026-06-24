using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Telemedicine.EndSession;

public sealed record EndTelemedicineSessionCommand(Guid AppointmentId) : ICommand<EndTelemedicineSessionDto>;
