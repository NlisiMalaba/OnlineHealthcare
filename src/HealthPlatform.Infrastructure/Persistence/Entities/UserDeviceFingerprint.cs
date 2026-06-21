namespace HealthPlatform.Infrastructure.Persistence.Entities;

public sealed class UserDeviceFingerprint
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    /// <summary>
    /// SHA-256 of the client-provided fingerprint material (hex, 64 chars).
    /// </summary>
    public string FingerprintHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset LastSeenAtUtc { get; set; }
}
