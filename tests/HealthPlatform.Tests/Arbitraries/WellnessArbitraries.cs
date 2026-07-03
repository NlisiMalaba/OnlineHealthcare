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

public sealed record ConsecutiveMissedDosesAlertCase(
    int NextOfKinContactCount,
    int ConsecutiveMissedDoseCount);

public enum EmergencyAlertTrigger
{
    Doctor = 0,
    System = 1
}

public sealed record EmergencyAlertCase(
    int NextOfKinContactCount,
    EmergencyAlertTrigger Trigger);

public enum NextOfKinRetryFailedChannel
{
    Sms = 0,
    Push = 1
}

public sealed record NextOfKinNotificationRetryCase(
    NextOfKinRetryFailedChannel FailedChannel,
    int SucceedsOnAttempt);

public static class WellnessArbitraries
{
    public static Arbitrary<MissedDoseDetectionCase> MissedDoseDetectionCase() =>
        Gen.OneOf(ShouldRecordMissedCase(), StillWithinGraceCase()).ToArbitrary();

    public static Arbitrary<ConsecutiveMissedDosesAlertCase> ConsecutiveMissedDosesAlertCase() =>
        (from contactCount in Gen.Choose(1, 5)
         from missedCount in Gen.Choose(3, 6)
         select new ConsecutiveMissedDosesAlertCase(contactCount, missedCount)).ToArbitrary();

    public static Arbitrary<EmergencyAlertCase> EmergencyAlertCase() =>
        (from contactCount in Gen.Choose(1, 3)
         from trigger in Gen.Elements(EmergencyAlertTrigger.Doctor, EmergencyAlertTrigger.System)
         select new EmergencyAlertCase(contactCount, trigger)).ToArbitrary();

    public static Arbitrary<NextOfKinNotificationRetryCase> NextOfKinNotificationRetryCase() =>
        (from failedChannel in Gen.Elements(NextOfKinRetryFailedChannel.Sms, NextOfKinRetryFailedChannel.Push)
         from succeedsOnAttempt in Gen.Choose(1, NextOfKinPoliciesMaxNotificationRetries + 1)
         select new NextOfKinNotificationRetryCase(failedChannel, succeedsOnAttempt)).ToArbitrary();

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

    private const int NextOfKinPoliciesMaxNotificationRetries = 3;
}
