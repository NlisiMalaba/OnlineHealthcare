using HealthPlatform.Domain.Wellness;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class HealthGoalConfiguration : IEntityTypeConfiguration<HealthGoal>
{
    public void Configure(EntityTypeBuilder<HealthGoal> builder)
    {
        builder.ToTable("health_goals");

        builder.HasKey(goal => goal.Id);

        builder.Property(goal => goal.MetricType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(goal => goal.TargetValue)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(goal => goal.Unit)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(goal => goal.CustomLabel)
            .HasMaxLength(100);

        builder.Property(goal => goal.Status)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.HasIndex(goal => goal.PatientId);
        builder.HasIndex(goal => new { goal.PatientId, goal.Status });
        builder.HasIndex(goal => new { goal.PatientId, goal.MetricType, goal.Status });
    }
}
