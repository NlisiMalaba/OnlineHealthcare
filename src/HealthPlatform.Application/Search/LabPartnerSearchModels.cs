namespace HealthPlatform.Application.Search;

public sealed record LabPartnerSearchCriteria(
    string? TestType,
    decimal? MinPrice,
    decimal? MaxPrice,
    double? PatientLatitude,
    double? PatientLongitude,
    int Page,
    int PageSize);

public sealed record LabPartnerSearchMatchDto(
    Guid LabPartnerId,
    string Name,
    string Address,
    IReadOnlyList<string> TestTypes,
    decimal? MatchingTestPrice,
    double? DistanceKilometers);

public sealed record LabPartnerSearchPageDto(
    IReadOnlyList<LabPartnerSearchMatchDto> Results,
    long TotalCount);
