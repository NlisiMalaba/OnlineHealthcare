using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness;

public static class AdherenceEventMappings
{
    public static AdherenceEventDto ToDto(this AdherenceEvent adherenceEvent) =>
        new(
            adherenceEvent.Id,
            adherenceEvent.ScheduleId,
            adherenceEvent.PatientId,
            adherenceEvent.ScheduledAtUtc,
            adherenceEvent.RecordedAtUtc,
            adherenceEvent.Status.ToString().ToLowerInvariant());
}
