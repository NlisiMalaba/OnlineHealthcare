using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HealthPlatform.Application.Insurance;
using HealthPlatform.Domain.Insurance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Insurance;

public sealed class RestInsurerApiClient(
    InsurerEndpointOptions settings,
    IHttpClientFactory httpClientFactory,
    ILogger<RestInsurerApiClient> logger) : IInsurerApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string InsurerCode => settings.Code.Trim().ToLowerInvariant();

    public async Task<InsurerClaimSubmissionResult> SubmitClaimAsync(
        InsurerClaimSubmissionRequest request,
        CancellationToken ct)
    {
        if (!IsConfigured())
        {
            return new InsurerClaimSubmissionResult(
                true,
                $"dev_{InsurerCode}_{request.ClaimId:N}",
                null,
                null);
        }

        var payload = new
        {
            external_reference = request.ClaimId,
            policy_number = request.PolicyNumber,
            member_number = request.MemberNumber,
            claim_type = request.ClaimType.ToString(),
            amount_minor_units = request.AmountMinorUnits,
            currency = request.Currency,
            appointment_id = request.AppointmentId,
            medication_order_id = request.MedicationOrderId,
            lab_order_id = request.LabOrderId
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "claims")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);

        var client = httpClientFactory.CreateClient(ClientName(InsurerCode));
        using var response = await client.SendAsync(httpRequest, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Insurer {InsurerCode} claim submission failed with status {StatusCode}.",
                InsurerCode,
                response.StatusCode);

            return new InsurerClaimSubmissionResult(
                false,
                null,
                InsuranceErrorCodes.InsurerUnavailable,
                "The insurer could not accept the claim.");
        }

        using var document = JsonDocument.Parse(body);
        var reference = document.RootElement.GetProperty("claim_reference").GetString();
        return new InsurerClaimSubmissionResult(true, reference, null, null);
    }

    public async Task<InsurerClaimStatusResult> GetClaimStatusAsync(
        string insurerClaimReference,
        CancellationToken ct)
    {
        if (!IsConfigured())
        {
            return new InsurerClaimStatusResult(
                true,
                InsuranceClaimStatus.Processing,
                null,
                null,
                null);
        }

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"claims/{Uri.EscapeDataString(insurerClaimReference)}");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);

        var client = httpClientFactory.CreateClient(ClientName(InsurerCode));
        using var response = await client.SendAsync(httpRequest, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            return new InsurerClaimStatusResult(
                false,
                null,
                null,
                InsuranceErrorCodes.InsurerUnavailable,
                "The insurer status endpoint is unavailable.");
        }

        using var document = JsonDocument.Parse(body);
        var statusText = document.RootElement.GetProperty("status").GetString();
        var reason = document.RootElement.TryGetProperty("status_reason", out var reasonElement)
            ? reasonElement.GetString()
            : null;

        if (!Enum.TryParse<InsuranceClaimStatus>(statusText, ignoreCase: true, out var status))
        {
            return new InsurerClaimStatusResult(
                false,
                null,
                null,
                InsuranceErrorCodes.InsurerUnavailable,
                "The insurer returned an unknown claim status.");
        }

        return new InsurerClaimStatusResult(true, status, reason, null, null);
    }

    public Task<InsurerWebhookParseResult> ParseStatusWebhookAsync(
        InsurerWebhookParseRequest request,
        CancellationToken ct)
    {
        if (!TryGetHeader(request.Headers, "x-insurer-signature", out var signature))
        {
            return Task.FromResult(new InsurerWebhookParseResult(false, null, null, null, null));
        }

        if (!IsConfigured())
        {
            if (!signature.StartsWith("dev:", StringComparison.Ordinal))
            {
                return Task.FromResult(new InsurerWebhookParseResult(false, null, null, null, null));
            }

            return Task.FromResult(ParseWebhookBody(request.RawBody) with { SignatureValid = true });
        }

        if (!VerifySignature(request.RawBody, signature, settings.WebhookSecret!))
        {
            return Task.FromResult(new InsurerWebhookParseResult(false, null, null, null, null));
        }

        return Task.FromResult(ParseWebhookBody(request.RawBody));
    }

    private bool IsConfigured() =>
        settings.Enabled
        && !string.IsNullOrWhiteSpace(settings.BaseUrl)
        && !string.IsNullOrWhiteSpace(settings.ApiKey)
        && !string.IsNullOrWhiteSpace(settings.WebhookSecret);

    private static InsurerWebhookParseResult ParseWebhookBody(string rawBody)
    {
        using var document = JsonDocument.Parse(rawBody);
        var root = document.RootElement;
        var eventId = root.GetProperty("event_id").GetString();
        var claimReference = root.GetProperty("claim_reference").GetString();
        var statusText = root.GetProperty("status").GetString();
        var reason = root.TryGetProperty("status_reason", out var reasonElement)
            ? reasonElement.GetString()
            : null;

        Enum.TryParse<InsuranceClaimStatus>(statusText, ignoreCase: true, out var status);

        return new InsurerWebhookParseResult(true, eventId, claimReference, status, reason);
    }

    private static bool VerifySignature(string payload, string signature, string webhookSecret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(webhookSecret));
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

    internal static string ClientName(string insurerCode) => $"insurer:{insurerCode}";
}
