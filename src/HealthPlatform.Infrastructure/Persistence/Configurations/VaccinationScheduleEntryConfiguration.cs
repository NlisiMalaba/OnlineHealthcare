using HealthPlatform.Domain.Vaccinations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class VaccinationScheduleEntryConfiguration : IEntityTypeConfiguration<VaccinationScheduleEntry>
{
    public void Configure(EntityTypeBuilder<VaccinationScheduleEntry> builder)
    {
        builder.ToTable("vaccination_schedule_entries");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.VaccineName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(entry => entry.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(entry => entry.ChildProfileId);
        builder.HasIndex(entry => entry.PatientId);
        builder.HasIndex(entry => new { entry.RecommendedDate, entry.ReminderSentAtUtc, entry.CompletedAtUtc });
    }
}
