using HealthPlatform.Domain.NextOfKin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class EmergencyAlertConfiguration : IEntityTypeConfiguration<EmergencyAlert>
{
    public void Configure(EntityTypeBuilder<EmergencyAlert> builder)
    {
        builder.ToTable("emergency_alerts");

        builder.HasKey(alert => alert.Id);

        builder.Property(alert => alert.TriggerReason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(alert => alert.TriggerSource)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(alert => alert.OverallStatus)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(alert => alert.PatientId);
        builder.HasIndex(alert => alert.TriggeredAtUtc);

        builder.Ignore(alert => alert.ContactDeliveries);
    }
}
