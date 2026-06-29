using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using HealthPlatform.Application.Payments;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Payments;

public sealed class MpesaPaymentGateway(
    IHttpClientFactory httpClientFactory,
    IOptions<PaymentGatewaysOptions> options,
    ILogger<MpesaPaymentGateway> logger) : IPaymentGateway
{
    private const string ApiBaseUrl = "https://sandbox.safaricom.co.ke/";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string ProviderName => PaymentGatewayProviders.Mpesa;

    public async Task<PaymentIntentResultDto> CreatePaymentIntentAsync(
        CreatePaymentIntentRequestDto request,
        CancellationToken ct)
    {
        var settings = options.Value.Mpesa;
        if (!IsConfigured(settings))
        {
            return CreateDevelopmentIntent(request);
        }

        var accessToken = await GetAccessTokenAsync(settings, ct);
        if (accessToken is null)
        {
            return new PaymentIntentResultDto(false, null, null, "MPESA_AUTH_FAILED", "M-Pesa authentication failed.");
        }

        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var password = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{settings.ShortCode}{settings.Passkey}{timestamp}"));

        var payload = new
        {
            BusinessShortCode = settings.ShortCode,
            Password = password,
            Timestamp = timestamp,
            TransactionType = "CustomerPayBillOnline",
            Amount = request.AmountMinorUnits / 100,
            PartyA = request.CustomerReference ?? "254700000000",
            PartyB = settings.ShortCode,
            PhoneNumber = request.CustomerReference ?? "254700000000",
            CallBackURL = settings.CallbackUrl,
            AccountReference = request.IdempotencyKey ?? $"hp_{Guid.NewGuid():N}",
            TransactionDesc = request.Description ?? "Healthcare payment"
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "mpesa/stkpush/v1/processrequest")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var client = httpClientFactory.CreateClient(nameof(MpesaPaymentGateway));
        using var response = await client.SendAsync(httpRequest, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("M-Pesa STK push failed with status {StatusCode}.", response.StatusCode);
            return new PaymentIntentResultDto(false, null, null, "MPESA_STK_FAILED", "M-Pesa STK push failed.");
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        return new PaymentIntentResultDto(
            true,
            root.GetProperty("CheckoutRequestID").GetString(),
            root.GetProperty("MerchantRequestID").GetString(),
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
        if (!IsConfigured(options.Value.Mpesa))
        {
            return Task.FromResult(ParseDevelopmentWebhook(request));
        }

        return Task.FromResult(ParseMpesaCallback(request.RawBody));
    }

    private async Task<string?> GetAccessTokenAsync(MpesaGatewayOptions settings, CancellationToken ct)
    {
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{settings.ConsumerKey}:{settings.ConsumerSecret}"));

        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, "oauth/v1/generate?grant_type=client_credentials");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        var client = httpClientFactory.CreateClient(nameof(MpesaPaymentGateway));
        using var response = await client.SendAsync(httpRequest, ct);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var body = await response.Content.ReadAsStringAsync(ct);
        using var document = JsonDocument.Parse(body);
        return document.RootElement.GetProperty("access_token").GetString();
    }

    private static PaymentIntentResultDto CreateDevelopmentIntent(CreatePaymentIntentRequestDto request)
    {
        var providerPaymentId = $"dev_mpesa_{Guid.NewGuid():N}";
        return new PaymentIntentResultDto(
            true,
            providerPaymentId,
            providerPaymentId,
            null,
            null);
    }

    private static bool IsConfigured(MpesaGatewayOptions settings) =>
        settings.Enabled
        && !string.IsNullOrWhiteSpace(settings.ConsumerKey)
        && !string.IsNullOrWhiteSpace(settings.ConsumerSecret)
        && !string.IsNullOrWhiteSpace(settings.Passkey)
        && !string.IsNullOrWhiteSpace(settings.ShortCode)
        && !string.IsNullOrWhiteSpace(settings.CallbackUrl);

    private static PaymentWebhookParseResultDto ParseDevelopmentWebhook(PaymentWebhookParseRequestDto request)
    {
        if (!request.Headers.TryGetValue("x-dev-mpesa", out var marker)
            || !string.Equals(marker, "true", StringComparison.OrdinalIgnoreCase))
        {
            return PaymentGatewayMetadataSupport.Ignored(signatureValid: false);
        }

        return ParseMpesaCallback(request.RawBody) with { SignatureValid = true };
    }

    private static PaymentWebhookParseResultDto ParseMpesaCallback(string rawBody)
    {
        using var document = JsonDocument.Parse(rawBody);
        var root = document.RootElement;
        var body = root.GetProperty("Body").GetProperty("stkCallback");
        var resultCode = body.GetProperty("ResultCode").GetInt32();
        var checkoutRequestId = body.GetProperty("CheckoutRequestID").GetString();
        var eventId = checkoutRequestId;

        if (resultCode != 0)
        {
            return new PaymentWebhookParseResultDto(
                true,
                eventId,
                checkoutRequestId,
                PaymentWebhookEventStatus.Failed,
                null,
                null,
                null,
                null,
                resultCode.ToString(),
                body.TryGetProperty("ResultDesc", out var resultDesc) ? resultDesc.GetString() : null);
        }

        long? amount = null;
        string? currency = "KES";
        Guid? appointmentId = null;
        if (body.TryGetProperty("CallbackMetadata", out var metadata)
            && metadata.TryGetProperty("Item", out var items)
            && items.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in items.EnumerateArray())
            {
                if (!item.TryGetProperty("Name", out var nameElement))
                {
                    continue;
                }

                var name = nameElement.GetString();
                if (string.Equals(name, "Amount", StringComparison.OrdinalIgnoreCase)
                    && item.TryGetProperty("Value", out var amountValue))
                {
                    amount = (long)(amountValue.GetDecimal() * 100m);
                }

                if (string.Equals(name, "AccountReference", StringComparison.OrdinalIgnoreCase)
                    && item.TryGetProperty("Value", out var accountReference)
                    && Guid.TryParse(accountReference.GetString(), out var parsedAppointmentId))
                {
                    appointmentId = parsedAppointmentId;
                }
            }
        }

        return new PaymentWebhookParseResultDto(
            true,
            eventId,
            checkoutRequestId,
            PaymentWebhookEventStatus.Completed,
            amount,
            currency,
            appointmentId,
            null,
            null,
            null);
    }
}
