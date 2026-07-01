using HealthPlatform.Domain.Wellness;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class MedicationDoseReminderConfiguration : IEntityTypeConfiguration<MedicationDoseReminder>
{
    public void Configure(EntityTypeBuilder<MedicationDoseReminder> builder)
    {
        builder.ToTable("medication_dose_reminders");

        builder.HasKey(reminder => reminder.Id);

        builder.HasIndex(reminder => new { reminder.ScheduleId, reminder.ScheduledAtUtc })
            .IsUnique();

        builder.HasIndex(reminder => reminder.PatientId);
        builder.HasIndex(reminder => reminder.ScheduledAtUtc);
    }
}
