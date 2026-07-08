using System.Text.Json;
using HealthPlatform.Domain.Referrals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class ReferralHealthRecordAccessGrantConfiguration
    : IEntityTypeConfiguration<ReferralHealthRecordAccessGrant>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<ReferralHealthRecordAccessGrant> builder)
    {
        builder.ToTable("referral_health_record_access_grants");

        builder.HasKey(x => x.Id);

        var sectionsConverter = new ValueConverter<IReadOnlyList<string>, string>(
            sections => JsonSerializer.Serialize(sections, SerializerOptions),
            value => JsonSerializer.Deserialize<string[]>(value, SerializerOptions) ?? Array.Empty<string>());

        builder.Property(x => x.SharedHealthRecordSections)
            .HasConversion(sectionsConverter)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasIndex(x => x.ReferralId).IsUnique();
        builder.HasIndex(x => x.PatientId);
        builder.HasIndex(x => x.DoctorId);
    }
}
