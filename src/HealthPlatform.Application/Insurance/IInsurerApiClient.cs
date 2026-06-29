using HealthPlatform.Domain.Insurance;

namespace HealthPlatform.Application.Insurance;

public sealed record InsurerClaimSubmissionRequest(
    Guid ClaimId,
    Guid PatientId,
    string PolicyNumber,
    string? MemberNumber,
    InsuranceClaimType ClaimType,
    long AmountMinorUnits,
    string Currency,
    Guid? AppointmentId,
    Guid? MedicationOrderId,
    Guid? LabOrderId);

public sealed record InsurerClaimSubmissionResult(
    bool Succeeded,
    string? InsurerClaimReference,
    string? FailureCode,
    string? FailureMessage);

public sealed record InsurerClaimStatusResult(
    bool Succeeded,
    InsuranceClaimStatus? Status,
    string? StatusReason,
    string? FailureCode,
    string? FailureMessage);

public sealed record InsurerWebhookParseRequest(
    string RawBody,
    IReadOnlyDictionary<string, string> Headers);

public sealed record InsurerWebhookParseResult(
    bool SignatureValid,
    string? EventId,
    string? InsurerClaimReference,
    InsuranceClaimStatus? Status,
    string? StatusReason);

public interface IInsurerApiClient
{
    string InsurerCode { get; }

    Task<InsurerClaimSubmissionResult> SubmitClaimAsync(
        InsurerClaimSubmissionRequest request,
        CancellationToken ct);

    Task<InsurerClaimStatusResult> GetClaimStatusAsync(
        string insurerClaimReference,
        CancellationToken ct);

    Task<InsurerWebhookParseResult> ParseStatusWebhookAsync(
        InsurerWebhookParseRequest request,
        CancellationToken ct);
}

public interface IInsurerApiClientResolver
{
    IInsurerApiClient GetRequired(string insurerCode);
}

public interface IInsuranceClaimWebhookIdempotencyStore
{
    Task<bool> TryBeginProcessingAsync(string insurerCode, string eventId, CancellationToken ct);
}

public interface IInsuranceClaimStatusPoller
{
    Task<int> PollPendingClaimsAsync(CancellationToken ct);
}
