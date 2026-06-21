using HealthPlatform.Application.Search.EventHandlers;
using HealthPlatform.Application.Search.Notifications;
using HealthPlatform.Domain.Identity.Events;
using HealthPlatform.Tests.Support;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Search;

public sealed class PharmacyStockChangedSearchIndexHandlerTests
{
    [Fact]
    public async Task Handle_UpdatesPharmacyStockIndex()
    {
        var pharmacyId = Guid.NewGuid();
        var searchService = new CapturingSearchService();
        var handler = new PharmacyStockChangedSearchIndexHandler(
            searchService,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<PharmacyStockChangedSearchIndexHandler>>());

        var stock = new List<PharmacyStockSummaryItem>
        {
            new("Paracetamol", "MED-001", 25)
        };

        await handler.Handle(
            new PharmacyStockChangedNotification(pharmacyId, stock, DateTime.UtcNow),
            CancellationToken.None);

        Assert.Single(searchService.PharmacyStockUpdates);
        Assert.Equal(pharmacyId, searchService.PharmacyStockUpdates[0].PharmacyId);
        Assert.Single(searchService.PharmacyStockUpdates[0].Stock);
        Assert.Equal("MED-001", searchService.PharmacyStockUpdates[0].Stock[0].MedicationSku);
    }
}
