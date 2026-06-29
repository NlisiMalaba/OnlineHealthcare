using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HealthPlatform.Application.Payments;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Payments;

public sealed class PaystackPaymentGateway(
    IHttpClientFactory httpClientFactory,
    IOptions<PaymentGatewaysOptions> options,
    ILogger<PaystackPaymentGateway> logger) : IPaymentGateway
{
    private const string ApiBaseUrl = "https://api.paystack.co/";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string ProviderName => PaymentGatewayProviders.Paystack;

    public async Task<PaymentIntentResultDto> CreatePaymentIntentAsync(
        CreatePaymentIntentRequestDto request,
        CancellationToken ct)
    {
        var settings = options.Value.Paystack;
        if (!IsConfigured(settings))
        {
            return CreateDevelopmentIntent(request);
        }

        var payload = new
        {
            email = request.CustomerReference ?? "patient@example.com",
            amount = request.AmountMinorUnits,
            currency = request.Currency.ToUpperInvariant(),
            reference = request.IdempotencyKey ?? $"hp_{Guid.NewGuid():N}",
            metadata = request.Metadata
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "transaction/initialize")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.SecretKey);

        var client = httpClientFactory.CreateClient(nameof(PaystackPaymentGateway));
        using var response = await client.SendAsync(httpRequest, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Paystack transaction initialization failed with status {StatusCode}.", response.StatusCode);
            return new PaymentIntentResultDto(false, null, null, "PAYSTACK_ERROR", "Paystack transaction initialization failed.");
        }

        using var document = JsonDocument.Parse(body);
        var data = document.RootElement.GetProperty("data");
        return new PaymentIntentResultDto(
            true,
            data.GetProperty("reference").GetString(),
            data.GetProperty("authorization_url").GetString(),
            null,
            null);
    }

    public Task<PaymentCaptureResultDto> CapturePaymentAsync(
        CapturePaymentRequestDto request,
        CancellationToken ct) =>
        Task.FromResult(new PaymentCaptureResultDto(true, null, null));

    public Task<PaymentWebhookParseResultDto> ParseWebhookAsync(
        PaymentWebhookParseRequestDto request,
        CancellationToken ct)
    {
        var settings = options.Value.Paystack;
        if (!TryGetHeader(request.Headers, "x-paystack-signature", out var signature))
        {
            return Task.FromResult(PaymentGatewayMetadataSupport.Ignored(signatureValid: false));
        }

        if (!IsConfigured(settings))
        {
            return Task.FromResult(ParseDevelopmentWebhook(request.RawBody, signature));
        }

        if (!VerifyPaystackSignature(request.RawBody, signature, settings.WebhookSecret!))
        {
            return Task.FromResult(PaymentGatewayMetadataSupport.Ignored(signatureValid: false));
        }

        return Task.FromResult(ParsePaystackEvent(request.RawBody));
    }

    private static PaymentIntentResultDto CreateDevelopmentIntent(CreatePaymentIntentRequestDto request)
    {
        var providerPaymentId = $"dev_psk_{Guid.NewGuid():N}";
        return new PaymentIntentResultDto(
            true,
            providerPaymentId,
            $"https://checkout.paystack.com/dev/{providerPaymentId}",
            null,
            null);
    }

    private static bool IsConfigured(PaystackGatewayOptions settings) =>
        settings.Enabled
        && !string.IsNullOrWhiteSpace(settings.SecretKey)
        && !string.IsNullOrWhiteSpace(settings.WebhookSecret);

    private static PaymentWebhookParseResultDto ParseDevelopmentWebhook(string rawBody, string signature)
    {
        if (!signature.StartsWith("dev:", StringComparison.Ordinal))
        {
            return PaymentGatewayMetadataSupport.Ignored(signatureValid: false);
        }

        return ParsePaystackEvent(rawBody) with { SignatureValid = true };
    }

    private static PaymentWebhookParseResultDto ParsePaystackEvent(string rawBody)
    {
        using var document = JsonDocument.Parse(rawBody);
        var root = document.RootElement;
        var eventType = root.GetProperty("event").GetString();
        if (!string.Equals(eventType, "charge.success", StringComparison.OrdinalIgnoreCase))
        {
            return PaymentGatewayMetadataSupport.Ignored(signatureValid: true);
        }

        var data = root.GetProperty("data");
        var eventId = data.GetProperty("id").GetRawText().Trim('"');
        var metadata = data.TryGetProperty("metadata", out var metadataElement) ? metadataElement : default;
        return new PaymentWebhookParseResultDto(
            true,
            eventId,
            data.GetProperty("reference").GetString(),
            PaymentWebhookEventStatus.Completed,
            data.GetProperty("amount").GetInt64(),
            data.GetProperty("currency").GetString(),
            PaymentGatewayMetadataSupport.TryReadGuidMetadata(metadata, PaymentMetadataKeys.AppointmentId),
            PaymentGatewayMetadataSupport.TryReadGuidMetadata(metadata, PaymentMetadataKeys.MedicationOrderId),
            null,
            null);
    }

    private static bool VerifyPaystackSignature(string payload, string signature, string webhookSecret)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(webhookSecret));
        var hash = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(hash),
            Encoding.UTF8.GetBytes(signature.ToLowerInvariant()));
    }

    private static bool TryGetHeader(
        IReadOnlyDictionary<string, string> headers,
        string headerName,
        out string value)
    {
        if (headers.TryGetValue(headerName, out value!))
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        var match = headers.FirstOrDefault(
            pair => string.Equals(pair.Key, headerName, StringComparison.OrdinalIgnoreCase));
        value = match.Value ?? string.Empty;
        return !string.IsNullOrWhiteSpace(value);
    }
}
