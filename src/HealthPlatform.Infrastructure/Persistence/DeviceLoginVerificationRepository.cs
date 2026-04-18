using HealthPlatform.Application.Auth;
using HealthPlatform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence;

public sealed class DeviceLoginVerificationRepository(ApplicationDbContext db) : IDeviceLoginVerificationRepository
{
    public async Task<Guid> CreateAsync(
        Guid userId,
        string fingerprintSha256Hex,
        string otpPasswordHash,
        DateTimeOffset expiresAtUtc,
        CancellationToken ct)
    {
        var id = Guid.CreateVersion7();
        db.DeviceLoginVerifications.Add(
            new DeviceLoginVerification
            {
                Id = id,
                UserId = userId,
                DeviceFingerprintHash = fingerprintSha256Hex,
                OtpPasswordHash = otpPasswordHash,
                ExpiresAtUtc = expiresAtUtc,
                ConsumedAtUtc = null
            });
        await db.SaveChangesAsync(ct);
        return id;
    }

    public async Task<DeviceLoginVerificationSnapshot?> FindConsumableAsync(
        Guid id,
        Guid userId,
        string fingerprintSha256Hex,
        CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var row = await db.DeviceLoginVerifications.FirstOrDefaultAsync(
            x => x.Id == id
                && x.UserId == userId
                && x.DeviceFingerprintHash == fingerprintSha256Hex
                && x.ConsumedAtUtc == null
                && x.ExpiresAtUtc > now,
            ct);

        return row is null ? null : new DeviceLoginVerificationSnapshot(row.Id, row.OtpPasswordHash);
    }

    public async Task MarkConsumedAsync(Guid id, CancellationToken ct)
    {
        await db.DeviceLoginVerifications
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ConsumedAtUtc, DateTimeOffset.UtcNow), ct);
    }
}
