using HealthPlatform.Application.Search;
using HealthPlatform.Application.Search.SearchLabPartners;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Search;

public sealed class SearchLabPartnersQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenNoResults_ReturnsEmptyStateMessageAndSuggestion()
    {
        var searchService = new Mock<ISearchService>();
        searchService
            .Setup(service => service.SearchLabPartnersAsync(It.IsAny<LabPartnerSearchCriteria>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LabPartnerSearchPageDto([], 0));

        var handler = new SearchLabPartnersQueryHandler(searchService.Object);

        var response = await handler.Handle(
            new SearchLabPartnersQuery(
                TestType: "CBC",
                MinPrice: 10m,
                MaxPrice: 50m,
                PatientLatitude: -17.8252,
                PatientLongitude: 31.0335),
            CancellationToken.None);

        Assert.Empty(response.Results);
        Assert.Equal(0, response.TotalCount);
        Assert.Equal("No lab partners match your current search filters.", response.EmptyStateMessage);
        Assert.Contains("widen the price range", response.EmptyStateSuggestion, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_WhenResultsExist_DoesNotReturnEmptyState()
    {
        var labPartnerId = Guid.NewGuid();
        var searchService = new Mock<ISearchService>();
        searchService
            .Setup(service => service.SearchLabPartnersAsync(It.IsAny<LabPartnerSearchCriteria>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LabPartnerSearchPageDto(
            [
                new LabPartnerSearchMatchDto(
                    labPartnerId,
                    "Metro Labs",
                    "456 Lab Avenue",
                    ["CBC", "Lipid Panel"],
                    25m,
                    3.4)
            ],
            1));

        var handler = new SearchLabPartnersQueryHandler(searchService.Object);

        var response = await handler.Handle(
            new SearchLabPartnersQuery(TestType: "CBC"),
            CancellationToken.None);

        Assert.Single(response.Results);
        Assert.Equal(labPartnerId, response.Results[0].LabPartnerId);
        Assert.Equal(25m, response.Results[0].MatchingTestPrice);
        Assert.Null(response.EmptyStateMessage);
        Assert.Null(response.EmptyStateSuggestion);
    }

    [Fact]
    public async Task Handle_PassesTrimmedFilterCriteriaToSearchService()
    {
        LabPartnerSearchCriteria? capturedCriteria = null;
        var searchService = new Mock<ISearchService>();
        searchService
            .Setup(service => service.SearchLabPartnersAsync(It.IsAny<LabPartnerSearchCriteria>(), It.IsAny<CancellationToken>()))
            .Callback<LabPartnerSearchCriteria, CancellationToken>((criteria, _) => capturedCriteria = criteria)
            .ReturnsAsync(new LabPartnerSearchPageDto([], 0));

        var handler = new SearchLabPartnersQueryHandler(searchService.Object);

        await handler.Handle(
            new SearchLabPartnersQuery(
                TestType: "  CBC  ",
                MinPrice: 15m,
                MaxPrice: 40m,
                PatientLatitude: -17.8,
                PatientLongitude: 31.0,
                Page: 3,
                PageSize: 15),
            CancellationToken.None);

        Assert.NotNull(capturedCriteria);
        Assert.Equal("CBC", capturedCriteria!.TestType);
        Assert.Equal(15m, capturedCriteria.MinPrice);
        Assert.Equal(40m, capturedCriteria.MaxPrice);
        Assert.Equal(-17.8, capturedCriteria.PatientLatitude);
        Assert.Equal(31.0, capturedCriteria.PatientLongitude);
        Assert.Equal(3, capturedCriteria.Page);
        Assert.Equal(15, capturedCriteria.PageSize);
    }
}
