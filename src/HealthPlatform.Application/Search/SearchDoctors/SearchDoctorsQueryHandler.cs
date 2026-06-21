using MediatR;

namespace HealthPlatform.Application.Search.SearchDoctors;

public sealed class SearchDoctorsQueryHandler(ISearchService searchService)
    : IRequestHandler<SearchDoctorsQuery, SearchDoctorsResponseDto>
{
    public async Task<SearchDoctorsResponseDto> Handle(SearchDoctorsQuery request, CancellationToken ct)
    {
        var criteria = new DoctorSearchCriteria(
            string.IsNullOrWhiteSpace(request.Specialty) ? null : request.Specialty.Trim(),
            request.MinRating,
            request.MinFee,
            request.MaxFee,
            request.HasAvailability,
            request.PatientLatitude,
            request.PatientLongitude,
            request.Page,
            request.PageSize);

        var page = await searchService.SearchDoctorsAsync(criteria, ct);

        if (page.Results.Count == 0)
        {
            return new SearchDoctorsResponseDto(
                [],
                page.TotalCount,
                DoctorSearchEmptyState.BuildMessage(request),
                DoctorSearchEmptyState.BuildSuggestion(request));
        }

        var results = page.Results
            .Select(match => new DoctorSearchResultDto(
                match.DoctorId,
                match.Name,
                match.Specialty,
                match.AverageRating,
                match.TotalReviews,
                match.VirtualFee,
                match.PhysicalFee,
                match.DistanceKilometers))
            .ToList();

        return new SearchDoctorsResponseDto(results, page.TotalCount, null, null);
    }
}
