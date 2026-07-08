using HealthPlatform.Domain.Referrals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class ReferralConfiguration : IEntityTypeConfiguration<Referral>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<Referral> builder)
    {
        builder.ToTable("referrals");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Reason)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(r => r.ReceivingHospitalName)
            .HasMaxLength(200);

        builder.Property(r => r.ClinicalNotes)
            .HasMaxLength(2000);

        builder.Property(r => r.ResponseReason)
            .HasMaxLength(1000);

        builder.Property(r => r.ConsultationSummaryEntryId)
            .HasMaxLength(200);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        var sectionsConverter = new ValueConverter<IReadOnlyList<string>, string>(
            sections => JsonSerializer.Serialize(sections, SerializerOptions),
            value => JsonSerializer.Deserialize<string[]>(value, SerializerOptions) ?? Array.Empty<string>());

        builder.Property(r => r.SharedHealthRecordSections)
            .HasConversion(sectionsConverter)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasIndex(r => r.PatientId);
        builder.HasIndex(r => r.ReferringDoctorId);
        builder.HasIndex(r => r.ReceivingDoctorId);
        builder.HasIndex(r => new { r.PatientId, r.Status });
    }
}
