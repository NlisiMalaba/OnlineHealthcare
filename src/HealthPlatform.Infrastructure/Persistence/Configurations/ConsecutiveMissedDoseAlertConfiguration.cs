using HealthPlatform.Domain.Wellness;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class ConsecutiveMissedDoseAlertConfiguration : IEntityTypeConfiguration<ConsecutiveMissedDoseAlert>
{
    public void Configure(EntityTypeBuilder<ConsecutiveMissedDoseAlert> builder)
    {
        builder.ToTable("consecutive_missed_dose_alerts");

        builder.HasKey(alert => alert.Id);

        builder.HasIndex(alert => new { alert.PatientId, alert.StreakEndScheduledAtUtc })
            .IsUnique();

        builder.HasIndex(alert => alert.TriggeringAdherenceEventId)
            .IsUnique();
    }
}
