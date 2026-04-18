namespace HealthPlatform.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "HealthPlatform";

    public string Audience { get; set; } = "HealthPlatform";

    /// <summary>
    /// Symmetric signing key (UTF-8); must be at least 32 bytes for HS256.
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 60;

    public int MfaChallengeMinutes { get; set; } = 5;
}
