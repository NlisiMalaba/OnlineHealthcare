using MediatR;

namespace HealthPlatform.Application.Search.SearchPharmacies;

public sealed record SearchPharmaciesQuery(
    string? MedicationSku = null,
    bool? HasStock = null,
    double? PatientLatitude = null,
    double? PatientLongitude = null,
    int Page = DoctorSearchOptions.DefaultPage,
    int PageSize = DoctorSearchOptions.DefaultPageSize) : IRequest<SearchPharmaciesResponseDto>;

public sealed record PharmacySearchResultDto(
    Guid PharmacyId,
    string Name,
    string Address,
    bool HasStock,
    double? DistanceKilometers);

public sealed record SearchPharmaciesResponseDto(
    IReadOnlyList<PharmacySearchResultDto> Results,
    long TotalCount,
    string? EmptyStateMessage,
    string? EmptyStateSuggestion);
