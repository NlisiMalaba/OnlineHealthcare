namespace HealthPlatform.Infrastructure.Auth;

public sealed class DeviceLoginOptions
{
    public const string SectionName = "DeviceLogin";

    /// <summary>
    /// Lifetime of the device step-up JWT and matching verification row.
    /// </summary>
    public int ChallengeTtlMinutes { get; set; } = 10;

    /// <summary>
    /// When true and the host environment is Development, logs the numeric code (never enable outside local dev).
    /// </summary>
    public bool LogOneTimeCodeInDevelopment { get; set; }
}
