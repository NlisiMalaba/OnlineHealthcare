using HealthPlatform.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class UserNotificationPreferenceConfiguration : IEntityTypeConfiguration<UserNotificationPreference>
{
    public void Configure(EntityTypeBuilder<UserNotificationPreference> builder)
    {
        builder.ToTable("user_notification_preferences");

        builder.HasKey(preference => preference.Id);

        builder.Property(preference => preference.EventType)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(preference => preference.Channel)
            .HasMaxLength(16)
            .IsRequired();

        builder.HasIndex(preference => new { preference.UserId, preference.EventType, preference.Channel })
            .IsUnique();
    }
}
