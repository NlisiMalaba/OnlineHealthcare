using HealthPlatform.Domain.Maternal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class AntenatalRecordConfiguration : IEntityTypeConfiguration<AntenatalRecord>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<AntenatalRecord> builder)
    {
        builder.ToTable("antenatal_records");

        builder.HasKey(record => record.Id);

        builder.Property(record => record.EstimatedDueDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(record => record.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        var entryRefsConverter = new ValueConverter<IReadOnlyList<string>, string>(
            refs => JsonSerializer.Serialize(refs, SerializerOptions),
            value => JsonSerializer.Deserialize<string[]>(value, SerializerOptions) ?? Array.Empty<string>());

        builder.Property(record => record.EntryRefs)
            .HasConversion(entryRefsConverter)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(record => record.NextReminderAtUtc);
        builder.Property(record => record.LastReminderSentAtUtc);

        builder.HasIndex(record => record.PatientId);
        builder.HasIndex(record => record.ObstetricDoctorId);
        builder.HasIndex(record => new { record.PatientId, record.Status });
        builder.HasIndex(record => record.NextReminderAtUtc);
    }
}
