using HealthPlatform.Domain.Wellness;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class MedicationScheduleConfiguration : IEntityTypeConfiguration<MedicationSchedule>
{
    public void Configure(EntityTypeBuilder<MedicationSchedule> builder)
    {
        builder.ToTable("medication_schedules");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.MedicationName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.PrimitiveCollection(s => s.DoseTimes)
            .HasColumnType("timestamp with time zone[]")
            .IsRequired();

        builder.HasIndex(s => s.PatientId);
        builder.HasIndex(s => new { s.PatientId, s.Status });
        builder.HasIndex(s => s.PrescriptionId);
    }
}
