using System.Text.Json;
using HealthPlatform.Domain.HealthRecords;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class HealthRecordAccessConfiguration : IEntityTypeConfiguration<HealthRecordAccess>
{
    private static readonly JsonSerializerOptions SerializerOptions = new();

    public void Configure(EntityTypeBuilder<HealthRecordAccess> builder)
    {
        builder.ToTable("health_record_accesses");

        builder.HasKey(access => access.Id);

        builder.Property(access => access.AccessType)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(access => access.Sections)
            .HasColumnType("text")
            .HasConversion(
                sections => JsonSerializer.Serialize(sections, SerializerOptions),
                json => JsonSerializer.Deserialize<List<string>>(json, SerializerOptions) ?? new List<string>());

        builder.HasIndex(access => access.HealthRecordId);
        builder.HasIndex(access => new { access.HealthRecordId, access.GrantedToDoctorId, access.RevokedAtUtc });
    }
}
