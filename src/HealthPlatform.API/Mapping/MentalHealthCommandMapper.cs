using HealthPlatform.API.Requests.MentalHealth;
using HealthPlatform.Application.MentalHealth.CompleteTherapySession;
using HealthPlatform.Application.MentalHealth.GrantTherapySessionBroaderAccess;

namespace HealthPlatform.API.Mapping;

public static class MentalHealthCommandMapper
{
    public static CompleteTherapySessionCommand ToCompleteCommand(
        Guid therapySessionId,
        CompleteTherapySessionRequest request) =>
        new(therapySessionId, request.SessionSummary);

    public static GrantTherapySessionBroaderAccessCommand ToGrantBroaderAccessCommand(Guid therapySessionId) =>
        new(therapySessionId);
}
