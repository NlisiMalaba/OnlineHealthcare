using HealthPlatform.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class LicenseVerificationQueueItemConfiguration
    : IEntityTypeConfiguration<LicenseVerificationQueueItem>
{
    public void Configure(EntityTypeBuilder<LicenseVerificationQueueItem> builder)
    {
        builder.ToTable("license_verification_queue");

        builder.HasKey(q => q.Id);

        builder.HasIndex(q => q.DoctorId)
            .IsUnique()
            .HasFilter("\"IsCompleted\" = false");
    }
}
