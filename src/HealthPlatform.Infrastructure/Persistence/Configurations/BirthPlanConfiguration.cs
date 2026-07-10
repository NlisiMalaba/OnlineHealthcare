using System.Text.Json;
using HealthPlatform.Domain.Maternal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class BirthPlanConfiguration : IEntityTypeConfiguration<BirthPlan>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<BirthPlan> builder)
    {
        builder.ToTable("birth_plans");

        builder.HasKey(plan => plan.Id);

        var contentConverter = new ValueConverter<BirthPlanContent, string>(
            content => JsonSerializer.Serialize(content, SerializerOptions),
            value => JsonSerializer.Deserialize<BirthPlanContent>(value, SerializerOptions)
                ?? new BirthPlanContent(null, null, null, null));

        builder.Property(plan => plan.Content)
            .HasConversion(contentConverter)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasIndex(plan => plan.PatientId);
        builder.HasIndex(plan => plan.AntenatalRecordId).IsUnique();
    }
}
