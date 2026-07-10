using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.MentalHealth.CompleteTherapySession;

public sealed record CompleteTherapySessionCommand(
    Guid TherapySessionId,
    string SessionSummary) : ICommand<TherapySessionDto>;
