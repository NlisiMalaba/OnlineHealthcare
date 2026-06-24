using HealthPlatform.Domain.Telemedicine;

namespace HealthPlatform.Application.HealthRecords;

public sealed record TelemedicineSessionSummaryRecord(
    Guid SessionId,
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    TelemedicineSessionMode Mode,
    int DurationSeconds,
    DateTime StartedAtUtc,
    DateTime EndedAtUtc,
    bool RecordingEnabled,
    string SummaryText);

public sealed record TelemedicineSessionSummaryReference(string DocumentId);

public interface ITelemedicineSessionSummaryRepository
{
    Task<TelemedicineSessionSummaryReference> SaveAsync(
        TelemedicineSessionSummaryRecord summary,
        CancellationToken ct);
}
