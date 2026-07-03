using HealthPlatform.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.ToTable("notification_logs");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.RecipientType)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(entry => entry.Channel)
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(entry => entry.EventType)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(entry => entry.PayloadJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(entry => entry.Status)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(entry => entry.FailureReason)
            .HasMaxLength(64);

        builder.HasIndex(entry => entry.RecipientId);
        builder.HasIndex(entry => entry.SentAtUtc);
        builder.HasIndex(entry => new { entry.EventType, entry.Channel, entry.Status });
    }
}
