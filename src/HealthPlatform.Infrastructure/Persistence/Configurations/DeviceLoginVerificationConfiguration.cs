using HealthPlatform.Infrastructure.Identity;
using HealthPlatform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class DeviceLoginVerificationConfiguration : IEntityTypeConfiguration<DeviceLoginVerification>
{
    public void Configure(EntityTypeBuilder<DeviceLoginVerification> builder)
    {
        builder.ToTable("device_login_verifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DeviceFingerprintHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.OtpPasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(x => x.ExpiresAtUtc).IsRequired();
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.ExpiresAtUtc });
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
