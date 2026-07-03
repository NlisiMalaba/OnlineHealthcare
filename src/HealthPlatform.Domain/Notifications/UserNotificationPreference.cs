using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Notifications;

public sealed class UserNotificationPreference : Entity
{
    private UserNotificationPreference()
    {
        EventType = string.Empty;
        Channel = string.Empty;
    }

    public Guid UserId { get; private set; }

    public string EventType { get; private set; }

    public string Channel { get; private set; }

    public bool IsEnabled { get; private set; }

    public static UserNotificationPreference Create(
        Guid userId,
        string eventType,
        string channel,
        bool isEnabled)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("Event type is required.", nameof(eventType));
        }

        if (string.IsNullOrWhiteSpace(channel))
        {
            throw new ArgumentException("Channel is required.", nameof(channel));
        }

        return new UserNotificationPreference
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            EventType = eventType.Trim(),
            Channel = channel.Trim().ToLowerInvariant(),
            IsEnabled = isEnabled
        };
    }

    public void SetEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
        Touch();
    }
}
