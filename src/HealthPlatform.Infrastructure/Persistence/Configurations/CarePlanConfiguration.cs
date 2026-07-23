using System.Text.Json;
using HealthPlatform.Domain.Wellness;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class CarePlanConfiguration : IEntityTypeConfiguration<CarePlan>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<CarePlan> builder)
    {
        builder.ToTable("care_plans");

        builder.HasKey(plan => plan.Id);

        builder.Property(plan => plan.Condition)
            .HasMaxLength(200)
            .IsRequired();

        var tasksConverter = new ValueConverter<IReadOnlyList<CarePlanTask>, string>(
            tasks => JsonSerializer.Serialize(tasks, SerializerOptions),
            value => JsonSerializer.Deserialize<List<CarePlanTask>>(value, SerializerOptions)
                ?? new List<CarePlanTask>());

        builder.Property(plan => plan.Tasks)
            .HasConversion(tasksConverter)
            .HasColumnType("jsonb")
            .IsRequired();

        var targetsConverter = new ValueConverter<IReadOnlyList<CarePlanMonitoringTarget>, string>(
            targets => JsonSerializer.Serialize(targets, SerializerOptions),
            value => JsonSerializer.Deserialize<List<CarePlanMonitoringTarget>>(value, SerializerOptions)
                ?? new List<CarePlanMonitoringTarget>());

        builder.Property(plan => plan.MonitoringTargets)
            .HasConversion(targetsConverter)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(plan => plan.ReviewIntervalDays)
            .IsRequired();

        builder.Property(plan => plan.NextReviewAt)
            .IsRequired();

        builder.Property(plan => plan.ReviewReminderSentAtUtc);

        builder.Property(plan => plan.Status)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Ignore(plan => plan.Progress);

        builder.HasIndex(plan => plan.PatientId);
        builder.HasIndex(plan => plan.DoctorId);
        builder.HasIndex(plan => new { plan.PatientId, plan.Status });
        builder.HasIndex(plan => new { plan.DoctorId, plan.Status });
        builder.HasIndex(plan => new { plan.Status, plan.NextReviewAt });
    }
}
