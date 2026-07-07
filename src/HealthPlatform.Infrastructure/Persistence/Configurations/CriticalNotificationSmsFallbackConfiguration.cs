using HealthPlatform.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class CriticalNotificationSmsFallbackConfiguration
    : IEntityTypeConfiguration<CriticalNotificationSmsFallback>
{
    public void Configure(EntityTypeBuilder<CriticalNotificationSmsFallback> builder)
    {
        builder.ToTable("critical_notification_sms_fallbacks");

        builder.HasKey(fallback => fallback.Id);

        builder.Property(fallback => fallback.RecipientType)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(fallback => fallback.EventType)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(fallback => fallback.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(fallback => fallback.Body)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(fallback => fallback.PayloadJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(fallback => fallback.Email)
            .HasMaxLength(320);

        builder.Property(fallback => fallback.PhoneNumberE164)
            .HasMaxLength(32);

        builder.Property(fallback => fallback.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(fallback => new { fallback.Status, fallback.NextRetryAtUtc });
        builder.HasIndex(fallback => fallback.RecipientId);
    }
}
