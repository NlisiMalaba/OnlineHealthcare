using HealthPlatform.Domain.NextOfKin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class NextOfKinNotificationDeliveryConfiguration
    : IEntityTypeConfiguration<NextOfKinNotificationDelivery>
{
    public void Configure(EntityTypeBuilder<NextOfKinNotificationDelivery> builder)
    {
        builder.ToTable("next_of_kin_notification_deliveries");

        builder.HasKey(delivery => delivery.Id);

        builder.Property(delivery => delivery.NotificationType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(delivery => delivery.Channel)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(delivery => delivery.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(delivery => new { delivery.Status, delivery.NextRetryAtUtc });
        builder.HasIndex(delivery => new
        {
            delivery.NotificationType,
            delivery.ReferenceId,
            delivery.NextOfKinContactId,
            delivery.Channel
        }).IsUnique();
    }
}
