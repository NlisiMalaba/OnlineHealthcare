namespace HealthPlatform.Application.HealthRecords;

public sealed record HealthRecordTelemedicineSummaryEntry(
    Guid HealthRecordId,
    Guid PatientId,
    Guid DoctorId,
    Guid SessionId,
    Guid AppointmentId,
    string SummaryDocumentId,
    DateTime CreatedAtUtc);

public sealed record HealthRecordEntryReference(string EntryDocumentId);

public sealed record HealthRecordReferralConsultationSummaryEntry(
    Guid HealthRecordId,
    Guid PatientId,
    Guid DoctorId,
    Guid ReferralId,
    string Summary,
    DateTime CreatedAtUtc);

public interface IHealthRecordEntryRepository
{
    Task<HealthRecordEntryReference> AddTelemedicineSessionSummaryEntryAsync(
        HealthRecordTelemedicineSummaryEntry entry,
        CancellationToken ct);

    Task<HealthRecordEntryReference> AddReferralConsultationSummaryEntryAsync(
        HealthRecordReferralConsultationSummaryEntry entry,
        CancellationToken ct);
}
