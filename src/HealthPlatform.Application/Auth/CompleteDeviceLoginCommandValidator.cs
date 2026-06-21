using FluentValidation;

namespace HealthPlatform.Application.Auth;

public sealed class CompleteDeviceLoginCommandValidator : AbstractValidator<CompleteDeviceLoginCommand>
{
    public CompleteDeviceLoginCommandValidator()
    {
        RuleFor(x => x.DeviceChallengeToken).NotEmpty().MaximumLength(4096);
        RuleFor(x => x.VerificationCode).NotEmpty().Length(6).Matches(@"^\d{6}$");
        RuleFor(x => x.DeviceFingerprint)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(512);
    }
}
