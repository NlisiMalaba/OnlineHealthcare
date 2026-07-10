using HealthPlatform.Domain.Maternal;

namespace HealthPlatform.Application.Maternal.GrowthEntries;

public static class GrowthEntryMappings
{
    public static GrowthEntryDto ToDto(
        this GrowthEntry entry,
        DateOnly dateOfBirth) =>
        ToDto(entry, ChildGrowthReferencePolicies.Assess(
            dateOfBirth,
            entry.RecordedAtUtc,
            entry.HeightCm,
            entry.WeightKg));

    public static GrowthEntryDto ToDto(
        GrowthEntry entry,
        ChildGrowthAssessmentResult assessment) =>
        new(
            entry.Id,
            entry.ChildProfileId,
            entry.HeightCm,
            entry.WeightKg,
            entry.MilestoneNote,
            entry.RecordedAtUtc,
            assessment.AgeMonths,
            assessment.HeightStatus,
            assessment.WeightStatus,
            assessment.HasOutOfRangeMeasurement);

    public static GrowthPercentileCurveDto ToDto(this ChildGrowthPercentileCurve curve) =>
        new(
            curve.Percentile,
            curve.Points
                .Select(point => new GrowthChartPointDto(point.AgeMonths, point.Value))
                .ToList());
}
