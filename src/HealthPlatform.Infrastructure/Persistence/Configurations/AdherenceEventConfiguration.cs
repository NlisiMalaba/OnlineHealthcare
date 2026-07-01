using HealthPlatform.Domain.Wellness;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class AdherenceEventConfiguration : IEntityTypeConfiguration<AdherenceEvent>
{
    public void Configure(EntityTypeBuilder<AdherenceEvent> builder)
    {
        builder.ToTable("adherence_events");

        builder.HasKey(adherenceEvent => adherenceEvent.Id);

        builder.Property(adherenceEvent => adherenceEvent.Status)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.HasIndex(adherenceEvent => new { adherenceEvent.ScheduleId, adherenceEvent.ScheduledAtUtc })
            .IsUnique();

        builder.HasIndex(adherenceEvent => adherenceEvent.PatientId);
        builder.HasIndex(adherenceEvent => new { adherenceEvent.PatientId, adherenceEvent.Status });
    }
}
