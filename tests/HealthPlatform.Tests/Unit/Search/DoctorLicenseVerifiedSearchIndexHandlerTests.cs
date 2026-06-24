using HealthPlatform.Application.Identity.Notifications;
using HealthPlatform.Application.Search.EventHandlers;
using HealthPlatform.Tests.Support;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Search;

public sealed class DoctorLicenseVerifiedSearchIndexHandlerTests
{
    [Fact]
    public async Task Handle_UpsertsDoctorIndex()
    {
        var doctorId = Guid.NewGuid();
        var searchService = new CapturingSearchService();
        var handler = new DoctorLicenseVerifiedSearchIndexHandler(
            searchService,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<DoctorLicenseVerifiedSearchIndexHandler>>());

        await handler.Handle(
            new DoctorLicenseVerifiedNotification(
                doctorId,
                Guid.NewGuid(),
                "Dr. Ada Lovelace",
                DateTime.UtcNow),
            CancellationToken.None);

        Assert.Single(searchService.DoctorUpserts);
        Assert.Equal(doctorId, searchService.DoctorUpserts[0]);
    }
}
