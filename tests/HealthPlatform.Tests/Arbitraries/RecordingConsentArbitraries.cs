using FsCheck;
using FsCheck.Xunit;

namespace HealthPlatform.Tests.Arbitraries;

public enum RecordingConsentStep
{
    GrantConsent = 0,
    JoinSession = 1,
    EnableRecording = 2
}

public sealed record RecordingConsentOperationSequence(IReadOnlyList<RecordingConsentStep> Steps);

public static class RecordingConsentArbitraries
{
    public static Arbitrary<RecordingConsentOperationSequence> RecordingConsentOperationSequence() =>
        Gen.ListOf(Gen.Elements(RecordingConsentStep.GrantConsent, RecordingConsentStep.JoinSession, RecordingConsentStep.EnableRecording))
            .Select(steps => new RecordingConsentOperationSequence([.. steps]))
            .ToArbitrary();
}
