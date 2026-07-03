using HealthPlatform.Domain.NextOfKin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class EmergencyAlertContactDeliveryConfiguration
    : IEntityTypeConfiguration<EmergencyAlertContactDelivery>
{
    public void Configure(EntityTypeBuilder<EmergencyAlertContactDelivery> builder)
    {
        builder.ToTable("emergency_alert_contact_deliveries");

        builder.HasKey(delivery => delivery.Id);

        builder.Property(delivery => delivery.SmsStatus)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(delivery => delivery.PushStatus)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.HasIndex(delivery => delivery.EmergencyAlertId);
        builder.HasIndex(delivery => new { delivery.EmergencyAlertId, delivery.NextOfKinContactId })
            .IsUnique();
    }
}
