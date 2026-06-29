namespace HealthPlatform.Application.Payments;

/// <summary>
/// Abstraction for payment providers (Stripe, Flutterwave, Paystack, M-Pesa, etc.).
/// Implementations live in Infrastructure with Polly policies and bounded timeouts.
/// </summary>
public interface IPaymentGateway
{
    string ProviderName { get; }

    Task<PaymentIntentResultDto> CreatePaymentIntentAsync(
        CreatePaymentIntentRequestDto request,
        CancellationToken ct);

    Task<PaymentCaptureResultDto> CapturePaymentAsync(
        CapturePaymentRequestDto request,
        CancellationToken ct);

    Task<PaymentWebhookParseResultDto> ParseWebhookAsync(
        PaymentWebhookParseRequestDto request,
        CancellationToken ct);
}

public sealed record CreatePaymentIntentRequestDto(
    string Currency,
    long AmountMinorUnits,
    string? CustomerReference,
    string? Description,
    string? IdempotencyKey,
    IReadOnlyDictionary<string, string>? Metadata);

public sealed record PaymentIntentResultDto(
    bool Succeeded,
    string? ProviderPaymentId,
    string? ClientSecret,
    string? FailureCode,
    string? FailureMessage);

public sealed record CapturePaymentRequestDto(string ProviderPaymentId, long? AmountMinorUnits);

public sealed record PaymentCaptureResultDto(
    bool Succeeded,
    string? FailureCode,
    string? FailureMessage);
