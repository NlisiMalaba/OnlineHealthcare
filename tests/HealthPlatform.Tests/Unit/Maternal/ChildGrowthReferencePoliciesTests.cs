using HealthPlatform.Domain.Maternal;
using Xunit;

namespace HealthPlatform.Tests.Unit.Maternal;

public sealed class ChildGrowthReferencePoliciesTests
{
    [Fact]
    public void Assess_marks_height_below_reference_range()
    {
        var dateOfBirth = new DateOnly(2025, 1, 1);
        var recordedAtUtc = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var assessment = ChildGrowthReferencePolicies.Assess(
            dateOfBirth,
            recordedAtUtc,
            50m,
            null);

        Assert.Equal(ChildGrowthMeasurementStatus.BelowRange, assessment.HeightStatus);
        Assert.True(assessment.HasOutOfRangeMeasurement);
    }

    [Fact]
    public void BuildHeightReferenceCurves_returns_percentile_series()
    {
        var curves = ChildGrowthReferencePolicies.BuildHeightReferenceCurves(24);

        Assert.Contains(curves, curve => curve.Percentile == 50);
        Assert.Equal(25, curves[0].Points.Count);
    }
}
