using HealthPlatform.Application.Search;
using HealthPlatform.Application.Search.SearchPharmacies;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Search;

public sealed class SearchPharmaciesQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenNoResults_ReturnsEmptyStateMessageAndSuggestion()
    {
        var searchService = new Mock<ISearchService>();
        searchService
            .Setup(service => service.SearchPharmaciesAsync(It.IsAny<PharmacySearchCriteria>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PharmacySearchPageDto([], 0));

        var handler = new SearchPharmaciesQueryHandler(searchService.Object);

        var response = await handler.Handle(
            new SearchPharmaciesQuery(
                MedicationSku: "MED-001",
                HasStock: true,
                PatientLatitude: -17.8252,
                PatientLongitude: 31.0335),
            CancellationToken.None);

        Assert.Empty(response.Results);
        Assert.Equal(0, response.TotalCount);
        Assert.Equal("No pharmacies match your current search filters.", response.EmptyStateMessage);
        Assert.Contains("different medication SKU", response.EmptyStateSuggestion, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_WhenResultsExist_DoesNotReturnEmptyState()
    {
        var pharmacyId = Guid.NewGuid();
        var searchService = new Mock<ISearchService>();
        searchService
            .Setup(service => service.SearchPharmaciesAsync(It.IsAny<PharmacySearchCriteria>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PharmacySearchPageDto(
            [
                new PharmacySearchMatchDto(
                    pharmacyId,
                    "City Pharmacy",
                    "123 Main Street",
                    true,
                    1.2)
            ],
            1));

        var handler = new SearchPharmaciesQueryHandler(searchService.Object);

        var response = await handler.Handle(
            new SearchPharmaciesQuery(MedicationSku: "MED-001"),
            CancellationToken.None);

        Assert.Single(response.Results);
        Assert.Equal(pharmacyId, response.Results[0].PharmacyId);
        Assert.Equal(1.2, response.Results[0].DistanceKilometers);
        Assert.Null(response.EmptyStateMessage);
        Assert.Null(response.EmptyStateSuggestion);
    }

    [Fact]
    public async Task Handle_PassesTrimmedFilterCriteriaToSearchService()
    {
        PharmacySearchCriteria? capturedCriteria = null;
        var searchService = new Mock<ISearchService>();
        searchService
            .Setup(service => service.SearchPharmaciesAsync(It.IsAny<PharmacySearchCriteria>(), It.IsAny<CancellationToken>()))
            .Callback<PharmacySearchCriteria, CancellationToken>((criteria, _) => capturedCriteria = criteria)
            .ReturnsAsync(new PharmacySearchPageDto([], 0));

        var handler = new SearchPharmaciesQueryHandler(searchService.Object);

        await handler.Handle(
            new SearchPharmaciesQuery(
                MedicationSku: "  MED-001  ",
                HasStock: true,
                PatientLatitude: -17.8,
                PatientLongitude: 31.0,
                Page: 2,
                PageSize: 10),
            CancellationToken.None);

        Assert.NotNull(capturedCriteria);
        Assert.Equal("MED-001", capturedCriteria!.MedicationSku);
        Assert.True(capturedCriteria.HasStock);
        Assert.Equal(-17.8, capturedCriteria.PatientLatitude);
        Assert.Equal(31.0, capturedCriteria.PatientLongitude);
        Assert.Equal(2, capturedCriteria.Page);
        Assert.Equal(10, capturedCriteria.PageSize);
    }
}
