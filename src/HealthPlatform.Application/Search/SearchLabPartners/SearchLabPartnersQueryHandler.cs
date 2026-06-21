using MediatR;

namespace HealthPlatform.Application.Search.SearchLabPartners;

public sealed class SearchLabPartnersQueryHandler(ISearchService searchService)
    : IRequestHandler<SearchLabPartnersQuery, SearchLabPartnersResponseDto>
{
    public async Task<SearchLabPartnersResponseDto> Handle(SearchLabPartnersQuery request, CancellationToken ct)
    {
        var criteria = new LabPartnerSearchCriteria(
            string.IsNullOrWhiteSpace(request.TestType) ? null : request.TestType.Trim(),
            request.MinPrice,
            request.MaxPrice,
            request.PatientLatitude,
            request.PatientLongitude,
            request.Page,
            request.PageSize);

        var page = await searchService.SearchLabPartnersAsync(criteria, ct);

        if (page.Results.Count == 0)
        {
            return new SearchLabPartnersResponseDto(
                [],
                page.TotalCount,
                LabPartnerSearchEmptyState.BuildMessage(request),
                LabPartnerSearchEmptyState.BuildSuggestion(request));
        }

        var results = page.Results
            .Select(match => new LabPartnerSearchResultDto(
                match.LabPartnerId,
                match.Name,
                match.Address,
                match.TestTypes,
                match.MatchingTestPrice,
                match.DistanceKilometers))
            .ToList();

        return new SearchLabPartnersResponseDto(results, page.TotalCount, null, null);
    }
}
