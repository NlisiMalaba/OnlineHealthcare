using HealthPlatform.Application.Search.EventHandlers;
using HealthPlatform.Application.Search.Notifications;
using HealthPlatform.Tests.Support;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Search;

public sealed class PharmacyProfileUpdatedSearchIndexHandlerTests
{
    [Fact]
    public async Task Handle_UpsertsPharmacyIndex()
    {
        var pharmacyId = Guid.NewGuid();
        var searchService = new CapturingSearchService();
        var handler = new PharmacyProfileUpdatedSearchIndexHandler(
            searchService,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<PharmacyProfileUpdatedSearchIndexHandler>>());

        await handler.Handle(
            new PharmacyProfileUpdatedNotification(pharmacyId, DateTime.UtcNow),
            CancellationToken.None);

        Assert.Single(searchService.PharmacyUpserts);
        Assert.Equal(pharmacyId, searchService.PharmacyUpserts[0]);
    }
}
