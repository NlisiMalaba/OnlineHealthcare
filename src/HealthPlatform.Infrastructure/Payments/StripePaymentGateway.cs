using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HealthPlatform.Application.Payments;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Payments;

public sealed class StripePaymentGateway(
    IHttpClientFactory httpClientFactory,
    IOptions<PaymentGatewaysOptions> options,
    ILogger<StripePaymentGateway> logger) : IPaymentGateway
{
    private const string ApiBaseUrl = "https://api.stripe.com/v1/";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string ProviderName => PaymentGatewayProviders.Stripe;

    public async Task<PaymentIntentResultDto> CreatePaymentIntentAsync(
        CreatePaymentIntentRequestDto request,
        CancellationToken ct)
    {
        var settings = options.Value.Stripe;
        if (!IsConfigured(settings))
        {
            return CreateDevelopmentIntent(request);
        }

        using var content = new FormUrlEncodedContent(BuildIntentForm(request));
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "payment_intents")
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.SecretKey);

        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            httpRequest.Headers.TryAddWithoutValidation("Idempotency-Key", request.IdempotencyKey);
        }

        var client = httpClientFactory.CreateClient(nameof(StripePaymentGateway));
        using var response = await client.SendAsync(httpRequest, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Stripe payment intent creation failed with status {StatusCode}.", response.StatusCode);
            return new PaymentIntentResultDto(false, null, null, "STRIPE_ERROR", "Stripe payment intent creation failed.");
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        return new PaymentIntentResultDto(
            true,
            root.GetProperty("id").GetString(),
            root.GetProperty("client_secret").GetString(),
            null,
            null);
    }

    public Task<PaymentCaptureResultDto> CapturePaymentAsync(
        CapturePaymentRequestDto request,
        CancellationToken ct)
    {
        var settings = options.Value.Stripe;
        if (!IsConfigured(settings))
        {
            return Task.FromResult(new PaymentCaptureResultDto(true, null, null));
        }

        return CaptureViaApiAsync(request, settings, ct);
    }

    public Task<PaymentWebhookParseResultDto> ParseWebhookAsync(
        PaymentWebhookParseRequestDto request,
        CancellationToken ct)
    {
        var settings = options.Value.Stripe;
        if (!TryGetHeader(request.Headers, "Stripe-Signature", out var signatureHeader))
        {
            return Task.FromResult(PaymentGatewayMetadataSupport.Ignored(signatureValid: false));
        }

        if (!IsConfigured(settings))
        {
            return Task.FromResult(ParseDevelopmentWebhook(request.RawBody, signatureHeader));
        }

        if (!VerifyStripeSignature(request.RawBody, signatureHeader, settings.WebhookSecret!))
        {
            return Task.FromResult(PaymentGatewayMetadataSupport.Ignored(signatureValid: false));
        }

        return Task.FromResult(ParseStripeEvent(request.RawBody));
    }

    private async Task<PaymentCaptureResultDto> CaptureViaApiAsync(
        CapturePaymentRequestDto request,
        StripeGatewayOptions settings,
        CancellationToken ct)
    {
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"payment_intents/{request.ProviderPaymentId}/capture");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.SecretKey);

        var client = httpClientFactory.CreateClient(nameof(StripePaymentGateway));
        using var response = await client.SendAsync(httpRequest, ct);
        if (response.IsSuccessStatusCode)
        {
            return new PaymentCaptureResultDto(true, null, null);
        }

        return new PaymentCaptureResultDto(false, "STRIPE_CAPTURE_FAILED", "Stripe capture failed.");
    }

    private static PaymentIntentResultDto CreateDevelopmentIntent(CreatePaymentIntentRequestDto request)
    {
        var providerPaymentId = $"dev_stripe_{Guid.NewGuid():N}";
        return new PaymentIntentResultDto(
            true,
            providerPaymentId,
            $"dev_secret_{providerPaymentId}",
            null,
            null);
    }

    private static IEnumerable<KeyValuePair<string, string>> BuildIntentForm(CreatePaymentIntentRequestDto request)
    {
        yield return new KeyValuePair<string, string>("amount", request.AmountMinorUnits.ToString());
        yield return new KeyValuePair<string, string>("currency", request.Currency.ToLowerInvariant());

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            yield return new KeyValuePair<string, string>("description", request.Description);
        }

        if (request.Metadata is not null)
        {
            foreach (var (key, value) in request.Metadata)
            {
                yield return new KeyValuePair<string, string>($"metadata[{key}]", value);
            }
        }
    }

    private static bool IsConfigured(StripeGatewayOptions settings) =>
        settings.Enabled
        && !string.IsNullOrWhiteSpace(settings.SecretKey)
        && !string.IsNullOrWhiteSpace(settings.WebhookSecret);

    private static PaymentWebhookParseResultDto ParseDevelopmentWebhook(string rawBody, string signatureHeader)
    {
        if (!signatureHeader.StartsWith("dev:", StringComparison.Ordinal))
        {
            return PaymentGatewayMetadataSupport.Ignored(signatureValid: false);
        }

        return ParseStripeEvent(rawBody) with { SignatureValid = true };
    }

    private static PaymentWebhookParseResultDto ParseStripeEvent(string rawBody)
    {
        using var document = JsonDocument.Parse(rawBody);
        var root = document.RootElement;
        var eventId = root.GetProperty("id").GetString();
        var eventType = root.GetProperty("type").GetString();
        var dataObject = root.GetProperty("data").GetProperty("object");

        return eventType switch
        {
            "payment_intent.succeeded" => CompletedFromStripe(eventId, dataObject),
            "payment_intent.payment_failed" => FailedFromStripe(eventId, dataObject),
            _ => PaymentGatewayMetadataSupport.Ignored(signatureValid: true)
        };
    }

    private static PaymentWebhookParseResultDto CompletedFromStripe(string? eventId, JsonElement dataObject)
    {
        var metadata = dataObject.TryGetProperty("metadata", out var metadataElement)
            ? metadataElement
            : default;

        return new PaymentWebhookParseResultDto(
            true,
            eventId,
            dataObject.GetProperty("id").GetString(),
            PaymentWebhookEventStatus.Completed,
            dataObject.GetProperty("amount").GetInt64(),
            dataObject.GetProperty("currency").GetString(),
            PaymentGatewayMetadataSupport.TryReadGuidMetadata(metadata, PaymentMetadataKeys.AppointmentId),
            PaymentGatewayMetadataSupport.TryReadGuidMetadata(metadata, PaymentMetadataKeys.MedicationOrderId),
            null,
            null);
    }

    private static PaymentWebhookParseResultDto FailedFromStripe(string? eventId, JsonElement dataObject)
    {
        string? failureCode = null;
        string? failureMessage = null;
        if (dataObject.TryGetProperty("last_payment_error", out var errorElement))
        {
            failureCode = errorElement.TryGetProperty("code", out var code) ? code.GetString() : null;
            failureMessage = errorElement.TryGetProperty("message", out var message) ? message.GetString() : null;
        }

        return new PaymentWebhookParseResultDto(
            true,
            eventId,
            dataObject.GetProperty("id").GetString(),
            PaymentWebhookEventStatus.Failed,
            dataObject.TryGetProperty("amount", out var amount) ? amount.GetInt64() : null,
            dataObject.TryGetProperty("currency", out var currency) ? currency.GetString() : null,
            null,
            null,
            failureCode,
            failureMessage);
    }

    private static bool VerifyStripeSignature(string payload, string signatureHeader, string webhookSecret)
    {
        var timestamp = signatureHeader
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => part.Split('=', 2))
            .FirstOrDefault(parts => parts.Length == 2 && parts[0] == "t")?[1];

        var signatures = signatureHeader
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => part.Split('=', 2))
            .Where(parts => parts.Length == 2 && parts[0] == "v1")
            .Select(parts => parts[1])
            .ToArray();

        if (timestamp is null || signatures.Length == 0)
        {
            return false;
        }

        var signedPayload = $"{timestamp}.{payload}";
        var keyBytes = Encoding.UTF8.GetBytes(webhookSecret);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload))).ToLowerInvariant();
        return signatures.Any(signature => CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(hash),
            Encoding.UTF8.GetBytes(signature.ToLowerInvariant())));
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
