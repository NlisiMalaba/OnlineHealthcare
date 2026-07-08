using HealthPlatform.Domain.HealthRecords;

namespace HealthPlatform.Application.HealthRecords;

public sealed record HealthRecordEntryReference(string EntryDocumentId);

public sealed record HealthRecordTelemedicineSummaryEntry(
    Guid HealthRecordId,
    Guid PatientId,
    Guid DoctorId,
    Guid SessionId,
    Guid AppointmentId,
    string SummaryDocumentId,
    DateTime CreatedAtUtc);

public sealed record HealthRecordEntryCreateModel(
    Guid HealthRecordId,
    HealthRecordEntryType EntryType,
    HealthRecordEntryContentPayload Content,
    Guid AuthoredBy,
    DateTime CreatedAtUtc,
    bool IsVisibleToPatient);

public sealed record HealthRecordEntryUpdateModel(
    string EntryId,
    HealthRecordEntryContentPayload Content,
    DateTime UpdatedAtUtc,
    bool? IsVisibleToPatient);

public sealed record HealthRecordReferralConsultationSummaryEntry(
    Guid HealthRecordId,
    Guid PatientId,
    Guid DoctorId,
    Guid ReferralId,
    string Summary,
    DateTime CreatedAtUtc);

public interface IHealthRecordEntryRepository
{
    Task<HealthRecordEntryDto> AddAsync(HealthRecordEntryCreateModel entry, CancellationToken ct);

    Task<HealthRecordEntryReference> AddTelemedicineSessionSummaryEntryAsync(
        HealthRecordTelemedicineSummaryEntry entry,
        CancellationToken ct);

    Task<HealthRecordEntryDto?> GetByIdAsync(string entryId, CancellationToken ct);

    Task<IReadOnlyList<HealthRecordEntryDto>> ListByHealthRecordIdAsync(
        Guid healthRecordId,
        bool patientVisibleOnly,
        CancellationToken ct);

    Task<bool> UpdateAsync(HealthRecordEntryUpdateModel entry, CancellationToken ct);

    Task<bool> DeleteAsync(string entryId, DateTime deletedAtUtc, CancellationToken ct);

    Task<HealthRecordEntryReference> AddReferralConsultationSummaryEntryAsync(
        HealthRecordReferralConsultationSummaryEntry entry,
        CancellationToken ct);
}
