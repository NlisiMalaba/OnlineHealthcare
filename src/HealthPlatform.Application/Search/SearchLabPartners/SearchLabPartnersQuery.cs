using MediatR;

namespace HealthPlatform.Application.Search.SearchLabPartners;

public sealed record SearchLabPartnersQuery(
    string? TestType = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    double? PatientLatitude = null,
    double? PatientLongitude = null,
    int Page = DoctorSearchOptions.DefaultPage,
    int PageSize = DoctorSearchOptions.DefaultPageSize) : IRequest<SearchLabPartnersResponseDto>;

public sealed record LabPartnerSearchResultDto(
    Guid LabPartnerId,
    string Name,
    string Address,
    IReadOnlyList<string> TestTypes,
    decimal? MatchingTestPrice,
    double? DistanceKilometers);

public sealed record SearchLabPartnersResponseDto(
    IReadOnlyList<LabPartnerSearchResultDto> Results,
    long TotalCount,
    string? EmptyStateMessage,
    string? EmptyStateSuggestion);
