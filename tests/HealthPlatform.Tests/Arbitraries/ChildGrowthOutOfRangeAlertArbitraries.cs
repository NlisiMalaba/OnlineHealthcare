using FsCheck;

namespace HealthPlatform.Tests.Arbitraries;

public enum ChildGrowthMeasurementExpectation
{
    OutOfRangeHeightBelow = 0,
    OutOfRangeHeightAbove = 1,
    OutOfRangeWeightBelow = 2,
    OutOfRangeWeightAbove = 3,
    InRange = 4
}

public sealed record ChildGrowthOutOfRangeAlertCase(
    int AgeMonths,
    ChildGrowthMeasurementExpectation Expectation);

public static class ChildGrowthOutOfRangeAlertArbitraries
{
    public static Arbitrary<ChildGrowthOutOfRangeAlertCase> ChildGrowthOutOfRangeAlertCase() =>
        (from ageMonths in Gen.Choose(0, 60)
         from expectation in Gen.Elements(
             ChildGrowthMeasurementExpectation.OutOfRangeHeightBelow,
             ChildGrowthMeasurementExpectation.OutOfRangeHeightAbove,
             ChildGrowthMeasurementExpectation.OutOfRangeWeightBelow,
             ChildGrowthMeasurementExpectation.OutOfRangeWeightAbove,
             ChildGrowthMeasurementExpectation.InRange)
         select new ChildGrowthOutOfRangeAlertCase(ageMonths, expectation))
        .ToArbitrary();
}
