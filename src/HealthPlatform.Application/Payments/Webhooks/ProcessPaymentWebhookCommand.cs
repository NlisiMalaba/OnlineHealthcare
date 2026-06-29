using MediatR;

namespace HealthPlatform.Application.Payments.Webhooks;

public sealed record ProcessPaymentWebhookCommand(
    string ProviderName,
    string RawBody,
    IReadOnlyDictionary<string, string> Headers) : IRequest<ProcessPaymentWebhookResultDto>;

public sealed record ProcessPaymentWebhookResultDto(
    bool Accepted,
    bool Duplicate,
    PaymentWebhookEventStatus? Status);
