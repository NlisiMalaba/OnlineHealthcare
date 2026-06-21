using HealthPlatform.Application.Search;
using HealthPlatform.Application.Search.SearchDoctors;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Search;

public sealed class SearchDoctorsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenNoResults_ReturnsEmptyStateMessageAndSuggestion()
    {
        var searchService = new Mock<ISearchService>();
        searchService
            .Setup(service => service.SearchDoctorsAsync(It.IsAny<DoctorSearchCriteria>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DoctorSearchPageDto([], 0));

        var handler = new SearchDoctorsQueryHandler(searchService.Object);

        var response = await handler.Handle(
            new SearchDoctorsQuery(
                Specialty: "Cardiology",
                MinRating: 4.5,
                MinFee: 100m,
                MaxFee: 200m,
                HasAvailability: true,
                PatientLatitude: -17.8252,
                PatientLongitude: 31.0335),
            CancellationToken.None);

        Assert.Empty(response.Results);
        Assert.Equal(0, response.TotalCount);
        Assert.Equal("No doctors match your current search filters.", response.EmptyStateMessage);
        Assert.Contains("widen the consultation fee range", response.EmptyStateSuggestion, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_WhenResultsExist_DoesNotReturnEmptyState()
    {
        var doctorId = Guid.NewGuid();
        var searchService = new Mock<ISearchService>();
        searchService
            .Setup(service => service.SearchDoctorsAsync(It.IsAny<DoctorSearchCriteria>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DoctorSearchPageDto(
            [
                new DoctorSearchMatchDto(
                    doctorId,
                    "Dr. Ada Lovelace",
                    "General Practice",
                    4.8m,
                    12,
                    50m,
                    80m,
                    2.5)
            ],
            1));

        var handler = new SearchDoctorsQueryHandler(searchService.Object);

        var response = await handler.Handle(
            new SearchDoctorsQuery(Specialty: "General Practice"),
            CancellationToken.None);

        Assert.Single(response.Results);
        Assert.Equal(doctorId, response.Results[0].DoctorId);
        Assert.Equal(2.5, response.Results[0].DistanceKilometers);
        Assert.Null(response.EmptyStateMessage);
        Assert.Null(response.EmptyStateSuggestion);
    }
}
