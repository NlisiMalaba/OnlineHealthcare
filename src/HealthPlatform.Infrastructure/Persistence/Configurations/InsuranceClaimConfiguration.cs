using HealthPlatform.Domain.Insurance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class InsuranceClaimConfiguration : IEntityTypeConfiguration<InsuranceClaim>
{
    public void Configure(EntityTypeBuilder<InsuranceClaim> builder)
    {
        builder.ToTable("insurance_claims");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.InsurerCode)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(c => c.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(c => c.ClaimType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(c => c.InsurerClaimReference)
            .HasMaxLength(128);

        builder.Property(c => c.StatusReason)
            .HasMaxLength(500);

        builder.HasIndex(c => c.PatientId);
        builder.HasIndex(c => new { c.InsurerCode, c.InsurerClaimReference }).IsUnique();
        builder.HasIndex(c => new { c.PatientId, c.ClaimType, c.AppointmentId, c.MedicationOrderId, c.LabOrderId })
            .IsUnique();
    }
}
