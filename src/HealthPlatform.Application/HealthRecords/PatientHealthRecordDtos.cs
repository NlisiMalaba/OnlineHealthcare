namespace HealthPlatform.Application.HealthRecords;

public sealed record PatientHealthRecordDto(
    Guid HealthRecordId,
    Guid PatientId,
    DateTime CreatedAtUtc,
    IReadOnlyList<HealthRecordEntryDto> Entries);

public sealed record HealthRecordPdfExportDto(
    Guid HealthRecordId,
    string DownloadUrl,
    DateTime GeneratedAtUtc);

public sealed record PatientHealthRecordExportModel(
    Guid HealthRecordId,
    Guid PatientId,
    string PatientDisplayName,
    DateTime GeneratedAtUtc,
    IReadOnlyList<HealthRecordEntryDto> Entries);

public interface IHealthRecordPdfGenerator
{
    byte[] Generate(PatientHealthRecordExportModel model);
}
