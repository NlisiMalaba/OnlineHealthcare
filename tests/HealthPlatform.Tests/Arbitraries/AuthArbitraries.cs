using FsCheck;

namespace HealthPlatform.Tests.Arbitraries;

public static class AuthArbitraries
{
    /// <summary>1–4 failed attempts (below lockout threshold).</summary>
    public static Arbitrary<int> PreLockoutFailureCount() =>
        Gen.Choose(1, 4).ToArbitrary();

    /// <summary>5–10 failed attempts (at or above lockout threshold).</summary>
    public static Arbitrary<int> LockoutFailureCount() =>
        Gen.Choose(5, 10).ToArbitrary();

    public static Arbitrary<int> RepeatLockedAttempts() =>
        Gen.Choose(0, 5).ToArbitrary();
}
