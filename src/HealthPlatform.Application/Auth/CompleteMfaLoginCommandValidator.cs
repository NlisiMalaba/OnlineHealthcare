using FluentValidation;

namespace HealthPlatform.Application.Auth;

public sealed class CompleteMfaLoginCommandValidator : AbstractValidator<CompleteMfaLoginCommand>
{
    public CompleteMfaLoginCommandValidator()
    {
        RuleFor(x => x.MfaChallengeToken).NotEmpty().MaximumLength(4096);
        RuleFor(x => x.TwoFactorCode).NotEmpty().MinimumLength(4).MaximumLength(12);
        RuleFor(x => x.TwoFactorProvider)
            .NotEmpty()
            .Must(p => p is TwoFactorProviders.Authenticator or TwoFactorProviders.Phone)
            .WithMessage($"Provider must be '{TwoFactorProviders.Authenticator}' or '{TwoFactorProviders.Phone}'.");
    }
}
