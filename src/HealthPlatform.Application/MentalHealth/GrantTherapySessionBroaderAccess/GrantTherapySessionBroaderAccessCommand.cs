using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.MentalHealth.GrantTherapySessionBroaderAccess;

public sealed record GrantTherapySessionBroaderAccessCommand(Guid TherapySessionId) : ICommand<TherapySessionDto>;
