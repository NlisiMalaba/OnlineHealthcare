using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.Notifications.UpdateNotificationPreferences;
using Xunit;

namespace HealthPlatform.Tests.Unit.Notifications;

public sealed class UpdateNotificationPreferencesCommandValidatorTests
{
    private readonly UpdateNotificationPreferencesCommandValidator _validator = new();

    [Fact]
    public void Validate_Rejects_unknown_channel()
    {
        var result = _validator.Validate(new UpdateNotificationPreferencesCommand(
        [
            new NotificationEventPreferenceUpdateDto(
                NotificationEventTypes.AppointmentConfirmed,
                [new NotificationChannelPreferenceUpdateDto("carrier_pigeon", false)])
        ]));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_Accepts_valid_preference_update()
    {
        var result = _validator.Validate(new UpdateNotificationPreferencesCommand(
        [
            new NotificationEventPreferenceUpdateDto(
                NotificationEventTypes.AppointmentConfirmed,
                [
                    new NotificationChannelPreferenceUpdateDto("push", true),
                    new NotificationChannelPreferenceUpdateDto("sms", false)
                ])
        ]));

        Assert.True(result.IsValid);
    }
}
