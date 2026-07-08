using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Labs.Webhooks;

public sealed record IngestLabResultWebhookCommand(
    string LabPartnerCode,
    string LabPartnerOrderReference,
    string TestCode,
    byte[] FileContent,
    string ContentType,
    string FileName,
    bool IsCritical) : ICommand<IngestLabResultWebhookResultDto>;

public sealed record IngestLabResultWebhookResultDto(
    bool Accepted,
    Guid LabResultId,
    Guid LabOrderId,
    string StorageKey);
