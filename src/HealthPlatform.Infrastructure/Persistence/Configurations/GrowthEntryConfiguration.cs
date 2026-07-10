using HealthPlatform.Domain.Maternal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class GrowthEntryConfiguration : IEntityTypeConfiguration<GrowthEntry>
{
    public void Configure(EntityTypeBuilder<GrowthEntry> builder)
    {
        builder.ToTable("growth_entries");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.HeightCm)
            .HasPrecision(6, 2);

        builder.Property(entry => entry.WeightKg)
            .HasPrecision(6, 3);

        builder.Property(entry => entry.MilestoneNote)
            .HasMaxLength(1000);

        builder.HasIndex(entry => entry.ChildProfileId);
        builder.HasIndex(entry => new { entry.ChildProfileId, entry.RecordedAtUtc });
    }
}
