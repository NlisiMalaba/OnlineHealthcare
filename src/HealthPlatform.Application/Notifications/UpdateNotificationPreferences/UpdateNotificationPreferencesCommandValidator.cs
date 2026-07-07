using FluentValidation;

namespace HealthPlatform.Application.Notifications.UpdateNotificationPreferences;

public sealed class UpdateNotificationPreferencesCommandValidator
    : AbstractValidator<UpdateNotificationPreferencesCommand>
{
    public UpdateNotificationPreferencesCommandValidator()
    {
        RuleFor(command => command.Preferences)
            .NotNull()
            .NotEmpty();

        RuleForEach(command => command.Preferences).ChildRules(preference =>
        {
            preference.RuleFor(item => item.EventType)
                .NotEmpty()
                .MaximumLength(128);

            preference.RuleFor(item => item.Channels)
                .NotNull()
                .NotEmpty();

            preference.RuleForEach(item => item.Channels).ChildRules(channel =>
            {
                channel.RuleFor(item => item.Channel)
                    .NotEmpty()
                    .MaximumLength(16)
                    .Must(channel => NotificationPreferenceDefaults.TryParseChannelKey(channel, out _))
                    .WithMessage("Channel must be one of: push, sms, email.");
            });
        });
    }
}
