using HealthPlatform.Domain.Wellness;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class WellnessEntryConfiguration : IEntityTypeConfiguration<WellnessEntry>
{
    public void Configure(EntityTypeBuilder<WellnessEntry> builder)
    {
        builder.ToTable("wellness_entries");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.MetricType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(entry => entry.Value)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.HasIndex(entry => entry.PatientId);
        builder.HasIndex(entry => entry.GoalId);
        builder.HasIndex(entry => new { entry.PatientId, entry.MetricType, entry.RecordedAtUtc });
    }
}
