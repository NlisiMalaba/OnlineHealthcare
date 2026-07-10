using HealthPlatform.Domain.Maternal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class AntenatalCheckupScheduleEntryConfiguration : IEntityTypeConfiguration<AntenatalCheckupScheduleEntry>
{
    public void Configure(EntityTypeBuilder<AntenatalCheckupScheduleEntry> builder)
    {
        builder.ToTable("antenatal_checkup_schedule_entries");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(entry => entry.CheckupEntryRef)
            .HasMaxLength(64);

        builder.Property(entry => entry.CompletedAtUtc);

        builder.Property(entry => entry.RecommendedDate)
            .HasColumnType("date")
            .IsRequired();

        builder.HasIndex(entry => entry.AntenatalRecordId);
        builder.HasIndex(entry => new { entry.AntenatalRecordId, entry.RecommendedDate });
    }
}
