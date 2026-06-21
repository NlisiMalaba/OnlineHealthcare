using MediatR;

namespace HealthPlatform.Application.Search.SearchDoctors;

public sealed record SearchDoctorsQuery(
    string? Specialty = null,
    double? MinRating = null,
    decimal? MinFee = null,
    decimal? MaxFee = null,
    bool? HasAvailability = null,
    double? PatientLatitude = null,
    double? PatientLongitude = null,
    int Page = DoctorSearchOptions.DefaultPage,
    int PageSize = DoctorSearchOptions.DefaultPageSize) : IRequest<SearchDoctorsResponseDto>;

public sealed record DoctorSearchResultDto(
    Guid DoctorId,
    string Name,
    string Specialty,
    decimal AverageRating,
    int TotalReviews,
    decimal VirtualFee,
    decimal PhysicalFee,
    double? DistanceKilometers);

public sealed record SearchDoctorsResponseDto(
    IReadOnlyList<DoctorSearchResultDto> Results,
    long TotalCount,
    string? EmptyStateMessage,
    string? EmptyStateSuggestion);
