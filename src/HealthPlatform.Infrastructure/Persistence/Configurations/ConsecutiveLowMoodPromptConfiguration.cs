using HealthPlatform.Domain.MentalHealth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class ConsecutiveLowMoodPromptConfiguration : IEntityTypeConfiguration<ConsecutiveLowMoodPrompt>
{
    public void Configure(EntityTypeBuilder<ConsecutiveLowMoodPrompt> builder)
    {
        builder.ToTable("consecutive_low_mood_prompts");

        builder.HasKey(prompt => prompt.Id);

        builder.Property(prompt => prompt.TriggeringMoodLogId)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(prompt => new { prompt.PatientId, prompt.TriggeringMoodLogId })
            .IsUnique();

        builder.HasIndex(prompt => prompt.PatientId);
    }
}
