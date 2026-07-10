namespace HealthPlatform.Application.MentalHealth;

public sealed record TherapySessionSummaryRecord(
    Guid TherapySessionId,
    Guid AppointmentId,
    Guid PatientId,
    Guid TherapistId,
    string SummaryText,
    DateTime CreatedAtUtc);

public sealed record TherapySessionSummaryReference(string DocumentId);

public interface ITherapySessionSummaryRepository
{
    Task<TherapySessionSummaryReference> SaveAsync(
        TherapySessionSummaryRecord summary,
        CancellationToken ct);
}
