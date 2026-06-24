using MediatR;

namespace HealthPlatform.Application.Search.SearchPharmacies;

public sealed class SearchPharmaciesQueryHandler(ISearchService searchService)
    : IRequestHandler<SearchPharmaciesQuery, SearchPharmaciesResponseDto>
{
    public async Task<SearchPharmaciesResponseDto> Handle(SearchPharmaciesQuery request, CancellationToken ct)
    {
        var criteria = new PharmacySearchCriteria(
            string.IsNullOrWhiteSpace(request.MedicationSku) ? null : request.MedicationSku.Trim(),
            request.HasStock,
            request.PatientLatitude,
            request.PatientLongitude,
            request.Page,
            request.PageSize);

        var page = await searchService.SearchPharmaciesAsync(criteria, ct);

        if (page.Results.Count == 0)
        {
            return new SearchPharmaciesResponseDto(
                [],
                page.TotalCount,
                PharmacySearchEmptyState.BuildMessage(request),
                PharmacySearchEmptyState.BuildSuggestion(request));
        }

        var results = page.Results
            .Select(match => new PharmacySearchResultDto(
                match.PharmacyId,
                match.Name,
                match.Address,
                match.HasStock,
                match.DistanceKilometers))
            .ToList();

        return new SearchPharmaciesResponseDto(results, page.TotalCount, null, null);
    }
}
