namespace HealthPlatform.Application.Labs;

public sealed record LabPartnerOrderSubmission(
    Guid LabOrderId,
    Guid PatientId,
    string LabPartnerCode,
    string TestCode,
    string? ClinicalNotes);

public interface ILabPartnerOrderClient
{
    Task<string> SubmitOrderAsync(LabPartnerOrderSubmission submission, CancellationToken ct);
}
