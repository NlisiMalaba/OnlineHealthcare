using HealthPlatform.Domain.Insurance;
using MediatR;

namespace HealthPlatform.Application.Insurance.Webhooks;

public sealed record ProcessInsuranceClaimWebhookCommand(
    string InsurerCode,
    string RawBody,
    IReadOnlyDictionary<string, string> Headers) : IRequest<ProcessInsuranceClaimWebhookResultDto>;

public sealed record ProcessInsuranceClaimWebhookResultDto(
    bool Accepted,
    bool Duplicate,
    InsuranceClaimStatus? Status);
