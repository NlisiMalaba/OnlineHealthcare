using HealthPlatform.Domain.MentalHealth;

namespace HealthPlatform.Application.MentalHealth;

public static class TherapySessionMappings
{
    public static TherapySessionDto ToDto(this TherapySession session) =>
        new(
            session.Id,
            session.AppointmentId,
            session.PatientId,
            session.TherapistId,
            session.SummaryRef,
            session.SummaryEntryId,
            session.IsVisibleToPatient,
            session.BroaderAccessGranted,
            session.Status.ToString().ToLowerInvariant(),
            session.CompletedAtUtc);
}
