using HealthPlatform.Domain.Prescriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.ToTable("prescriptions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.MedicationName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Dosage)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Frequency)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.SpecialInstructions)
            .HasMaxLength(1000);

        builder.Property(p => p.CancellationReason)
            .HasMaxLength(500);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(p => p.DoctorId);
        builder.HasIndex(p => p.PatientId);
        builder.HasIndex(p => p.HealthRecordId);
        builder.HasIndex(p => new { p.PatientId, p.Status });
    }
}
