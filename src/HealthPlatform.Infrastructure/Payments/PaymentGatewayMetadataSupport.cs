using System.Text.Json;
using HealthPlatform.Application.Payments;

namespace HealthPlatform.Infrastructure.Payments;

internal static class PaymentGatewayMetadataSupport
{
    public static Guid? TryReadGuidMetadata(JsonElement metadata, string key)
    {
        if (metadata.ValueKind != JsonValueKind.Object
            || !metadata.TryGetProperty(key, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String when Guid.TryParse(value.GetString(), out var parsed) => parsed,
            _ => null
        };
    }

    public static Guid? TryReadGuidMetadata(IReadOnlyDictionary<string, string>? metadata, string key)
    {
        if (metadata is null
            || !metadata.TryGetValue(key, out var value)
            || !Guid.TryParse(value, out var parsed))
        {
            return null;
        }

        return parsed;
    }

    public static PaymentWebhookParseResultDto Ignored(bool signatureValid) =>
        new(signatureValid, null, null, PaymentWebhookEventStatus.Ignored, null, null, null, null, null, null);
}
