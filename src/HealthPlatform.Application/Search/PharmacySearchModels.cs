namespace HealthPlatform.Application.Search;

public sealed record PharmacySearchCriteria(
    string? MedicationSku,
    bool? HasStock,
    double? PatientLatitude,
    double? PatientLongitude,
    int Page,
    int PageSize);

public sealed record PharmacySearchMatchDto(
    Guid PharmacyId,
    string Name,
    string Address,
    bool HasStock,
    double? DistanceKilometers);

public sealed record PharmacySearchPageDto(
    IReadOnlyList<PharmacySearchMatchDto> Results,
    long TotalCount);
