namespace HealthPlatform.Application.Auth;

public interface IUserDeviceFingerprintRepository
{
    Task<bool> ExistsAsync(Guid userId, string fingerprintSha256Hex, CancellationToken ct);

    /// <summary>
    /// Inserts a new trusted fingerprint or updates <c>LastSeenAtUtc</c> for an existing one.
    /// </summary>
    Task UpsertTouchAsync(Guid userId, string fingerprintSha256Hex, CancellationToken ct);
}
