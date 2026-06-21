using HealthPlatform.Domain.HealthRecords;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class HealthRecordConfiguration : IEntityTypeConfiguration<HealthRecord>
{
    public void Configure(EntityTypeBuilder<HealthRecord> builder)
    {
        builder.ToTable("health_records");

        builder.HasKey(r => r.Id);

        builder.HasIndex(r => r.PatientId)
            .IsUnique();

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
