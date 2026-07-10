using HealthPlatform.Domain.Vaccinations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class VaccinationRecordConfiguration : IEntityTypeConfiguration<VaccinationRecord>
{
    public void Configure(EntityTypeBuilder<VaccinationRecord> builder)
    {
        builder.ToTable("vaccination_records");

        builder.HasKey(record => record.Id);

        builder.Property(record => record.VaccineName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(record => record.BatchNumber)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(record => record.Provider)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(record => record.ChildProfileId);
        builder.HasIndex(record => record.PatientId);
        builder.HasIndex(record => record.ScheduleEntryId);
    }
}
