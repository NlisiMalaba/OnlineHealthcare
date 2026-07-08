using HealthPlatform.Domain.Labs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class LabOrderConfiguration : IEntityTypeConfiguration<LabOrder>
{
    public void Configure(EntityTypeBuilder<LabOrder> builder)
    {
        builder.ToTable("lab_orders");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RequestSource)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.LabPartnerCode)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.TestCode)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.ClinicalNotes)
            .HasMaxLength(1000);

        builder.Property(x => x.LabPartnerOrderReference)
            .HasMaxLength(128);

        builder.HasIndex(x => x.PatientId);
        builder.HasIndex(x => x.HealthRecordId);
    }
}
