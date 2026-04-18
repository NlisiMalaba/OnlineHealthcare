using HealthPlatform.Infrastructure.Identity;
using HealthPlatform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class UserDeviceFingerprintConfiguration : IEntityTypeConfiguration<UserDeviceFingerprint>
{
    public void Configure(EntityTypeBuilder<UserDeviceFingerprint> builder)
    {
        builder.ToTable("user_device_fingerprints");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FingerprintHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.LastSeenAtUtc).IsRequired();
        builder.HasIndex(x => new { x.UserId, x.FingerprintHash }).IsUnique();
        builder.HasIndex(x => x.UserId);
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
