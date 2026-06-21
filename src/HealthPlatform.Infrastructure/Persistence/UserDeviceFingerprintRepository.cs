using HealthPlatform.Application.Auth;
using HealthPlatform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence;

public sealed class UserDeviceFingerprintRepository(ApplicationDbContext db) : IUserDeviceFingerprintRepository
{
    public Task<bool> ExistsAsync(Guid userId, string fingerprintSha256Hex, CancellationToken ct) =>
        db.UserDeviceFingerprints.AnyAsync(
            x => x.UserId == userId && x.FingerprintHash == fingerprintSha256Hex,
            ct);

    public async Task UpsertTouchAsync(Guid userId, string fingerprintSha256Hex, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var existing = await db.UserDeviceFingerprints.FirstOrDefaultAsync(
            x => x.UserId == userId && x.FingerprintHash == fingerprintSha256Hex,
            ct);

        if (existing is null)
        {
            db.UserDeviceFingerprints.Add(
                new UserDeviceFingerprint
                {
                    Id = Guid.CreateVersion7(),
                    UserId = userId,
                    FingerprintHash = fingerprintSha256Hex,
                    CreatedAtUtc = now,
                    LastSeenAtUtc = now
                });
        }
        else
        {
            existing.LastSeenAtUtc = now;
        }

        await db.SaveChangesAsync(ct);
    }
}
