using FsCheck;

namespace HealthPlatform.Tests.Arbitraries;

public enum MissedDoseDetectionExpectation
{
    RecordsMissed = 0,
    WithinGracePeriod = 1
}

public sealed record MissedDoseDetectionCase(
    MissedDoseDetectionExpectation Expectation,
    int OffsetMinutes);

public static class WellnessArbitraries
{
    public static Arbitrary<MissedDoseDetectionCase> MissedDoseDetectionCase() =>
        Gen.OneOf(ShouldRecordMissedCase(), StillWithinGraceCase()).ToArbitrary();

    private static Gen<MissedDoseDetectionCase> ShouldRecordMissedCase() =>
        from offsetMinutes in Gen.Choose(0, 180)
        select new MissedDoseDetectionCase(
            MissedDoseDetectionExpectation.RecordsMissed,
            offsetMinutes);

    private static Gen<MissedDoseDetectionCase> StillWithinGraceCase() =>
        from offsetMinutes in Gen.Choose(1, 119)
        select new MissedDoseDetectionCase(
            MissedDoseDetectionExpectation.WithinGracePeriod,
            offsetMinutes);
}
