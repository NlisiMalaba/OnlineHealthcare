using HealthPlatform.Domain.Labs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class LabResultConfiguration : IEntityTypeConfiguration<LabResult>
{
    public void Configure(EntityTypeBuilder<LabResult> builder)
    {
        builder.ToTable("lab_results");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LabPartnerCode)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.LabPartnerOrderReference)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.TestCode)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.StorageKey)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.FileName)
            .HasMaxLength(260)
            .IsRequired();

        builder.HasIndex(x => x.LabOrderId);
        builder.HasIndex(x => x.PatientId);
        builder.HasIndex(x => new { x.LabPartnerCode, x.LabPartnerOrderReference });
    }
}
