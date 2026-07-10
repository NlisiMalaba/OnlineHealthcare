using HealthPlatform.Domain.Maternal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class ChildProfileConfiguration : IEntityTypeConfiguration<ChildProfile>
{
    public void Configure(EntityTypeBuilder<ChildProfile> builder)
    {
        builder.ToTable("child_profiles");

        builder.HasKey(profile => profile.Id);

        builder.Property(profile => profile.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(profile => profile.BloodType)
            .HasMaxLength(16);

        builder.PrimitiveCollection(profile => profile.KnownAllergies);

        builder.HasIndex(profile => profile.GuardianId);

        builder.HasIndex(profile => profile.HealthRecordId)
            .IsUnique();
    }
}
