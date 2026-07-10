namespace HealthPlatform.Domain.Maternal;

public enum ChildGrowthMeasurementStatus
{
    NotProvided = 0,
    InRange = 1,
    BelowRange = 2,
    AboveRange = 3
}

public readonly record struct ChildGrowthReferenceBand(
    decimal LowerBound,
    decimal UpperBound);

public readonly record struct ChildGrowthAssessmentResult(
    int AgeMonths,
    ChildGrowthMeasurementStatus HeightStatus,
    ChildGrowthMeasurementStatus WeightStatus)
{
    public bool HasOutOfRangeMeasurement =>
        HeightStatus is ChildGrowthMeasurementStatus.BelowRange or ChildGrowthMeasurementStatus.AboveRange
        || WeightStatus is ChildGrowthMeasurementStatus.BelowRange or ChildGrowthMeasurementStatus.AboveRange;
}

public static class ChildGrowthReferencePolicies
{
    public const int LowerPercentile = 3;

    public const int UpperPercentile = 97;

    private static readonly int[] ChartPercentiles = [3, 15, 50, 85, 97];

    private static readonly GrowthReferencePoint[] HeightForAgeReference =
    [
        new(0, 46.1m, 49.9m, 53.7m),
        new(3, 55.3m, 59.4m, 63.5m),
        new(6, 61.2m, 65.7m, 70.3m),
        new(9, 66.0m, 70.6m, 75.3m),
        new(12, 69.6m, 74.0m, 78.5m),
        new(18, 75.0m, 80.0m, 85.2m),
        new(24, 80.0m, 85.7m, 91.4m),
        new(36, 88.4m, 95.0m, 101.8m),
        new(48, 95.0m, 102.3m, 109.6m),
        new(60, 100.7m, 108.7m, 116.9m)
    ];

    private static readonly GrowthReferencePoint[] WeightForAgeReference =
    [
        new(0, 2.5m, 3.3m, 4.4m),
        new(3, 4.4m, 5.8m, 7.5m),
        new(6, 5.7m, 7.3m, 9.3m),
        new(9, 6.9m, 8.6m, 10.7m),
        new(12, 7.7m, 9.4m, 11.5m),
        new(18, 8.6m, 10.5m, 12.8m),
        new(24, 9.5m, 11.8m, 14.3m),
        new(36, 11.0m, 13.7m, 16.8m),
        new(48, 12.3m, 15.5m, 19.2m),
        new(60, 13.7m, 17.2m, 21.5m)
    ];

    public static int CalculateAgeMonths(DateOnly dateOfBirth, DateTime recordedAtUtc)
    {
        var recordedDate = DateOnly.FromDateTime(recordedAtUtc);
        var ageMonths = (recordedDate.Year - dateOfBirth.Year) * 12
            + recordedDate.Month - dateOfBirth.Month;

        if (recordedDate.Day < dateOfBirth.Day)
        {
            ageMonths--;
        }

        return Math.Max(ageMonths, 0);
    }

    public static ChildGrowthAssessmentResult Assess(
        DateOnly dateOfBirth,
        DateTime recordedAtUtc,
        decimal? heightCm,
        decimal? weightKg)
    {
        var ageMonths = CalculateAgeMonths(dateOfBirth, recordedAtUtc);

        var heightStatus = heightCm.HasValue
            ? AssessMeasurement(heightCm.Value, ageMonths, HeightForAgeReference)
            : ChildGrowthMeasurementStatus.NotProvided;

        var weightStatus = weightKg.HasValue
            ? AssessMeasurement(weightKg.Value, ageMonths, WeightForAgeReference)
            : ChildGrowthMeasurementStatus.NotProvided;

        return new ChildGrowthAssessmentResult(ageMonths, heightStatus, weightStatus);
    }

    public static IReadOnlyList<ChildGrowthPercentileCurve> BuildHeightReferenceCurves(int maxAgeMonths) =>
        BuildReferenceCurves(HeightForAgeReference, maxAgeMonths);

    public static IReadOnlyList<ChildGrowthPercentileCurve> BuildWeightReferenceCurves(int maxAgeMonths) =>
        BuildReferenceCurves(WeightForAgeReference, maxAgeMonths);

    private static ChildGrowthMeasurementStatus AssessMeasurement(
        decimal value,
        int ageMonths,
        GrowthReferencePoint[] referenceTable)
    {
        var band = GetPercentileBand(ageMonths, referenceTable);
        if (value < band.LowerBound)
        {
            return ChildGrowthMeasurementStatus.BelowRange;
        }

        if (value > band.UpperBound)
        {
            return ChildGrowthMeasurementStatus.AboveRange;
        }

        return ChildGrowthMeasurementStatus.InRange;
    }

    private static ChildGrowthReferenceBand GetPercentileBand(int ageMonths, GrowthReferencePoint[] referenceTable)
    {
        var lower = InterpolatePercentile(ageMonths, referenceTable, LowerPercentile);
        var upper = InterpolatePercentile(ageMonths, referenceTable, UpperPercentile);
        return new ChildGrowthReferenceBand(lower, upper);
    }

    private static IReadOnlyList<ChildGrowthPercentileCurve> BuildReferenceCurves(
        GrowthReferencePoint[] referenceTable,
        int maxAgeMonths)
    {
        var cappedMaxAge = Math.Clamp(maxAgeMonths, 0, 60);
        var curves = new List<ChildGrowthPercentileCurve>();

        foreach (var percentile in ChartPercentiles)
        {
            var points = new List<ChildGrowthChartPoint>();
            for (var ageMonths = 0; ageMonths <= cappedMaxAge; ageMonths++)
            {
                points.Add(new ChildGrowthChartPoint(
                    ageMonths,
                    InterpolatePercentile(ageMonths, referenceTable, percentile)));
            }

            curves.Add(new ChildGrowthPercentileCurve(percentile, points));
        }

        return curves;
    }

    private static decimal InterpolatePercentile(
        int ageMonths,
        GrowthReferencePoint[] referenceTable,
        int percentile)
    {
        if (ageMonths <= referenceTable[0].AgeMonths)
        {
            return referenceTable[0].GetPercentile(percentile);
        }

        for (var index = 1; index < referenceTable.Length; index++)
        {
            var previous = referenceTable[index - 1];
            var current = referenceTable[index];
            if (ageMonths > current.AgeMonths)
            {
                continue;
            }

            var span = current.AgeMonths - previous.AgeMonths;
            var offset = ageMonths - previous.AgeMonths;
            var ratio = span == 0 ? 0m : (decimal)offset / span;
            var previousValue = previous.GetPercentile(percentile);
            var currentValue = current.GetPercentile(percentile);
            return previousValue + ((currentValue - previousValue) * ratio);
        }

        return referenceTable[^1].GetPercentile(percentile);
    }

    private readonly record struct GrowthReferencePoint(int AgeMonths, decimal P3, decimal P50, decimal P97)
    {
        public decimal GetPercentile(int percentile) => percentile switch
        {
            3 => P3,
            15 => P3 + ((P50 - P3) * 0.375m),
            50 => P50,
            85 => P50 + ((P97 - P50) * 0.625m),
            97 => P97,
            _ => P50
        };
    }
}

public readonly record struct ChildGrowthChartPoint(int AgeMonths, decimal Value);

public readonly record struct ChildGrowthPercentileCurve(
    int Percentile,
    IReadOnlyList<ChildGrowthChartPoint> Points);
