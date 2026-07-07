using HealthPlatform.Domain.Labs;

namespace HealthPlatform.Application.Labs;

public sealed record LabResultDownloadDto(
    Guid LabResultId,
    string FileName,
    string ContentType,
    string DownloadUrl);

public sealed record LabOrderDto(
    Guid Id,
    Guid PatientId,
    Guid HealthRecordId,
    Guid? OrderingDoctorId,
    LabOrderRequestSource RequestSource,
    LabOrderStatus Status,
    string LabPartnerCode,
    string TestCode,
    string? ClinicalNotes,
    string? LabPartnerOrderReference,
    DateTime CreatedAtUtc,
    DateTime? ApprovedAtUtc);
