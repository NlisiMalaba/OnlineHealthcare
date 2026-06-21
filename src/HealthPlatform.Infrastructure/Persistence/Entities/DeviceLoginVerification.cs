namespace HealthPlatform.Infrastructure.Persistence.Entities;

public sealed class DeviceLoginVerification
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string DeviceFingerprintHash { get; set; } = string.Empty;

    /// <summary>
    /// ASP.NET Identity-style hash of the one-time numeric code (never store plaintext).
    /// </summary>
    public string OtpPasswordHash { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public DateTimeOffset? ConsumedAtUtc { get; set; }
}
