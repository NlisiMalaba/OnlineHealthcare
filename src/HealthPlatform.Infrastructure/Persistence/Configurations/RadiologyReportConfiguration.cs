using HealthPlatform.Domain.Labs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class RadiologyReportConfiguration : IEntityTypeConfiguration<RadiologyReport>
{
    public void Configure(EntityTypeBuilder<RadiologyReport> builder)
    {
        builder.ToTable("radiology_reports");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LabPartnerCode)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.LabPartnerOrderReference)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.ReportStorageKey)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.ReportContentType)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.ReportFileName)
            .HasMaxLength(260)
            .IsRequired();

        builder.Property(x => x.ImagingStorageKeysJson)
            .HasMaxLength(4000)
            .IsRequired();

        builder.HasIndex(x => x.LabOrderId);
        builder.HasIndex(x => x.PatientId);
        builder.HasIndex(x => new { x.LabPartnerCode, x.LabPartnerOrderReference });
    }
}
