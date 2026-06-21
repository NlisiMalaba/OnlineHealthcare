using HealthPlatform.Domain.HealthRecords;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class HealthRecordProfileChangeConfiguration : IEntityTypeConfiguration<HealthRecordProfileChange>
{
    public void Configure(EntityTypeBuilder<HealthRecordProfileChange> builder)
    {
        builder.ToTable("health_record_profile_changes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.FieldName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.PreviousValue)
            .HasMaxLength(4000);

        builder.Property(c => c.NewValue)
            .HasMaxLength(4000);

        builder.HasIndex(c => c.HealthRecordId);
        builder.HasIndex(c => new { c.PatientId, c.ChangedAtUtc });
    }
}
