using HealthPlatform.Infrastructure.Payments;
using Xunit;

namespace HealthPlatform.Tests.Unit.Payments;

public sealed class PaymentWebhookIdempotencyStoreTests
{
    [Fact]
    public async Task TryBeginProcessingAsync_allows_first_event_and_rejects_duplicates_per_provider()
    {
        var store = new InMemoryPaymentWebhookIdempotencyStore();

        var firstStripe = await store.TryBeginProcessingAsync(
            "stripe",
            "evt_123",
            CancellationToken.None);
        var duplicateStripe = await store.TryBeginProcessingAsync(
            "stripe",
            "evt_123",
            CancellationToken.None);
        var otherProviderSameEvent = await store.TryBeginProcessingAsync(
            "flutterwave",
            "evt_123",
            CancellationToken.None);

        Assert.True(firstStripe);
        Assert.False(duplicateStripe);
        Assert.True(otherProviderSameEvent);
    }
}
