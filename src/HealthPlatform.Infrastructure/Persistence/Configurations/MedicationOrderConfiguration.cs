using HealthPlatform.Domain.Pharmacy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class MedicationOrderConfiguration : IEntityTypeConfiguration<MedicationOrder>
{
    public void Configure(EntityTypeBuilder<MedicationOrder> builder)
    {
        builder.ToTable("medication_orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.MedicationSku)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(o => o.MedicationName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(o => o.Dosage)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(o => o.Frequency)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(o => o.SpecialInstructions)
            .HasMaxLength(1000);

        builder.Property(o => o.DeliveryAddress)
            .HasMaxLength(500);

        builder.Property(o => o.DeliveryType)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(o => o.DeliveryAgentName)
            .HasMaxLength(200);

        builder.Property(o => o.TrackingUrl)
            .HasMaxLength(500);

        builder.Property(o => o.RejectionReason)
            .HasMaxLength(500);

        builder.Property(o => o.ClarificationMessage)
            .HasMaxLength(1000);

        builder.HasIndex(o => o.PatientId);
        builder.HasIndex(o => o.PharmacyId);
        builder.HasIndex(o => o.PrescriptionId).IsUnique();
    }
}
