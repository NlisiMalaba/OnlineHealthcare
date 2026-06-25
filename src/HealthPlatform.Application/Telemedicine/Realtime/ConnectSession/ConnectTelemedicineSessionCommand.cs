using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Telemedicine.Realtime.ConnectSession;

public sealed record ConnectTelemedicineSessionCommand(Guid AppointmentId) : ICommand<ConnectTelemedicineSessionDto>;
