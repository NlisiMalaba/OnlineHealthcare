using HealthPlatform.Infrastructure.Identity;
using HealthPlatform.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<UserDeviceFingerprint> UserDeviceFingerprints => Set<UserDeviceFingerprint>();

    public DbSet<DeviceLoginVerification> DeviceLoginVerifications => Set<DeviceLoginVerification>();

    public DbSet<DomainEventOutboxEntry> DomainEventOutbox => Set<DomainEventOutboxEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
