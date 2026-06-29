namespace HealthPlatform.Application.Payments;

public interface IPaymentWebhookIdempotencyStore
{
    /// <summary>
    /// Returns <see langword="true"/> when the event has not been processed yet and is now reserved.
    /// </summary>
    Task<bool> TryBeginProcessingAsync(string provider, string eventId, CancellationToken ct);
}
