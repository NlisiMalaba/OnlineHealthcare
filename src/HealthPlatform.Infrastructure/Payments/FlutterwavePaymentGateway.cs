using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HealthPlatform.Application.Payments;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Payments;

public sealed class FlutterwavePaymentGateway(
    IHttpClientFactory httpClientFactory,
    IOptions<PaymentGatewaysOptions> options,
    ILogger<FlutterwavePaymentGateway> logger) : IPaymentGateway
{
    private const string ApiBaseUrl = "https://api.flutterwave.com/v3/";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string ProviderName => PaymentGatewayProviders.Flutterwave;

    public async Task<PaymentIntentResultDto> CreatePaymentIntentAsync(
        CreatePaymentIntentRequestDto request,
        CancellationToken ct)
    {
        var settings = options.Value.Flutterwave;
        if (!IsConfigured(settings))
        {
            return CreateDevelopmentIntent(request);
        }

        var payload = new
        {
            tx_ref = request.IdempotencyKey ?? $"hp_{Guid.NewGuid():N}",
            amount = request.AmountMinorUnits / 100m,
            currency = request.Currency.ToUpperInvariant(),
            redirect_url = "https://localhost/payments/flutterwave/return",
            customer = new
            {
                email = request.CustomerReference ?? "patient@example.com"
            },
            customizations = new
            {
                title = "HealthPlatform",
                description = request.Description ?? "Healthcare payment"
            },
            meta = request.Metadata
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "payments")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.SecretKey);

        var client = httpClientFactory.CreateClient(nameof(FlutterwavePaymentGateway));
        using var response = await client.SendAsync(httpRequest, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Flutterwave payment creation failed with status {StatusCode}.", response.StatusCode);
            return new PaymentIntentResultDto(false, null, null, "FLUTTERWAVE_ERROR", "Flutterwave payment creation failed.");
        }

        using var document = JsonDocument.Parse(body);
        var data = document.RootElement.GetProperty("data");
        return new PaymentIntentResultDto(
            true,
            data.GetProperty("id").GetRawText().Trim('"'),
            data.GetProperty("link").GetString(),
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
        var settings = options.Value.Flutterwave;
        if (!TryGetHeader(request.Headers, "verif-hash", out var signature))
        {
            return Task.FromResult(PaymentGatewayMetadataSupport.Ignored(signatureValid: false));
        }

        if (!IsConfigured(settings))
        {
            return Task.FromResult(ParseDevelopmentWebhook(request.RawBody, signature));
        }

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(signature),
                Encoding.UTF8.GetBytes(settings.WebhookSecret!)))
        {
            return Task.FromResult(PaymentGatewayMetadataSupport.Ignored(signatureValid: false));
        }

        return Task.FromResult(ParseFlutterwaveEvent(request.RawBody));
    }

    private static PaymentIntentResultDto CreateDevelopmentIntent(CreatePaymentIntentRequestDto request)
    {
        var providerPaymentId = $"dev_flw_{Guid.NewGuid():N}";
        return new PaymentIntentResultDto(
            true,
            providerPaymentId,
            $"https://checkout.flutterwave.com/dev/{providerPaymentId}",
            null,
            null);
    }

    private static bool IsConfigured(FlutterwaveGatewayOptions settings) =>
        settings.Enabled
        && !string.IsNullOrWhiteSpace(settings.SecretKey)
        && !string.IsNullOrWhiteSpace(settings.WebhookSecret);

    private static PaymentWebhookParseResultDto ParseDevelopmentWebhook(string rawBody, string signature)
    {
        if (!signature.StartsWith("dev:", StringComparison.Ordinal))
        {
            return PaymentGatewayMetadataSupport.Ignored(signatureValid: false);
        }

        return ParseFlutterwaveEvent(rawBody) with { SignatureValid = true };
    }

    private static PaymentWebhookParseResultDto ParseFlutterwaveEvent(string rawBody)
    {
        using var document = JsonDocument.Parse(rawBody);
        var root = document.RootElement;
        var eventType = root.TryGetProperty("event", out var eventElement) ? eventElement.GetString() : null;
        if (!string.Equals(eventType, "charge.completed", StringComparison.OrdinalIgnoreCase))
        {
            return PaymentGatewayMetadataSupport.Ignored(signatureValid: true);
        }

        var data = root.GetProperty("data");
        var status = data.GetProperty("status").GetString();
        var eventId = data.TryGetProperty("id", out var idElement)
            ? idElement.GetRawText().Trim('"')
            : data.GetProperty("tx_ref").GetString();

        if (string.Equals(status, "successful", StringComparison.OrdinalIgnoreCase))
        {
            return CompletedFromFlutterwave(eventId, data);
        }

        return new PaymentWebhookParseResultDto(
            true,
            eventId,
            data.TryGetProperty("flw_ref", out var flwRef) ? flwRef.GetString() : null,
            PaymentWebhookEventStatus.Failed,
            null,
            data.TryGetProperty("currency", out var currency) ? currency.GetString() : null,
            null,
            null,
            status,
            "Flutterwave charge was not successful.");
    }

    private static PaymentWebhookParseResultDto CompletedFromFlutterwave(string? eventId, JsonElement data)
    {
        var metadata = data.TryGetProperty("meta", out var metaElement) ? metaElement : default;
        var amount = data.TryGetProperty("amount", out var amountElement)
            ? (long)(amountElement.GetDecimal() * 100m)
            : (long?)null;

        return new PaymentWebhookParseResultDto(
            true,
            eventId,
            data.TryGetProperty("flw_ref", out var flwRef) ? flwRef.GetString() : null,
            PaymentWebhookEventStatus.Completed,
            amount,
            data.TryGetProperty("currency", out var currency) ? currency.GetString() : null,
            PaymentGatewayMetadataSupport.TryReadGuidMetadata(metadata, PaymentMetadataKeys.AppointmentId),
            PaymentGatewayMetadataSupport.TryReadGuidMetadata(metadata, PaymentMetadataKeys.MedicationOrderId),
            null,
            null);
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
