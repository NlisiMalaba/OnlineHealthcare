namespace HealthPlatform.Application.Auth;

public interface IDeviceLoginVerificationRepository
{
    Task<Guid> CreateAsync(
        Guid userId,
        string fingerprintSha256Hex,
        string otpPasswordHash,
        DateTimeOffset expiresAtUtc,
        CancellationToken ct);

    Task<DeviceLoginVerificationSnapshot?> FindConsumableAsync(
        Guid id,
        Guid userId,
        string fingerprintSha256Hex,
        CancellationToken ct);

    Task MarkConsumedAsync(Guid id, CancellationToken ct);
}
