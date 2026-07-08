using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Labs.Webhooks;

public sealed record RadiologyImagingUploadPayload(
    byte[] FileContent,
    string ContentType,
    string FileName);

public sealed record IngestRadiologyReportWebhookCommand(
    string LabPartnerCode,
    string LabPartnerOrderReference,
    byte[] ReportFileContent,
    string ReportContentType,
    string ReportFileName,
    IReadOnlyList<RadiologyImagingUploadPayload> ImagingFiles) : ICommand<IngestRadiologyReportWebhookResultDto>;

public sealed record IngestRadiologyReportWebhookResultDto(
    bool Accepted,
    Guid RadiologyReportId,
    Guid LabOrderId,
    string ReportStorageKey,
    int ImagingFileCount);
