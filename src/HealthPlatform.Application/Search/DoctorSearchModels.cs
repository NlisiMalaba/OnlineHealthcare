namespace HealthPlatform.Application.Search;

public static class DoctorSearchOptions
{
    public const int DefaultPage = 1;

    public const int DefaultPageSize = 20;

    public const int MaxPageSize = 50;
}

public sealed record DoctorSearchCriteria(
    string? Specialty,
    double? MinRating,
    decimal? MinFee,
    decimal? MaxFee,
    bool? HasAvailability,
    double? PatientLatitude,
    double? PatientLongitude,
    int Page,
    int PageSize);

public sealed record DoctorSearchMatchDto(
    Guid DoctorId,
    string Name,
    string Specialty,
    decimal AverageRating,
    int TotalReviews,
    decimal VirtualFee,
    decimal PhysicalFee,
    double? DistanceKilometers);

public sealed record DoctorSearchPageDto(
    IReadOnlyList<DoctorSearchMatchDto> Results,
    long TotalCount);
