using HealthPlatform.Domain.Maternal;

namespace HealthPlatform.Application.Maternal.GrowthEntries;

public sealed record GrowthEntryDto(
    Guid Id,
    Guid ChildProfileId,
    decimal? HeightCm,
    decimal? WeightKg,
    string? MilestoneNote,
    DateTime RecordedAtUtc,
    int AgeMonthsAtRecording,
    ChildGrowthMeasurementStatus HeightStatus,
    ChildGrowthMeasurementStatus WeightStatus,
    bool IsOutOfRange);

public sealed record GrowthChartPointDto(int AgeMonths, decimal Value);

public sealed record GrowthPercentileCurveDto(
    int Percentile,
    IReadOnlyList<GrowthChartPointDto> Points);

public sealed record GrowthChartDto(
    Guid ChildProfileId,
    DateOnly DateOfBirth,
    IReadOnlyList<GrowthPercentileCurveDto> HeightReferenceCurves,
    IReadOnlyList<GrowthPercentileCurveDto> WeightReferenceCurves,
    IReadOnlyList<GrowthEntryDto> Entries);
