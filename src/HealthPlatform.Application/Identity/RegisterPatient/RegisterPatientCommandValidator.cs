using FluentValidation;
using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Identity.RegisterPatient;

public sealed class RegisterPatientCommandValidator : AbstractValidator<RegisterPatientCommand>
{
    private static readonly System.Text.RegularExpressions.Regex E164PhonePattern =
        new(@"^\+[1-9]\d{1,14}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public RegisterPatientCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.AuthProvider)
            .IsInEnum();

        When(x => x.AuthProvider == PatientAuthProvider.Phone, () =>
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .Must(phone => E164PhonePattern.IsMatch(phone!))
                .WithMessage("Phone number must be in E.164 format (e.g. +263771234567).");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(12)
                .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain a digit.")
                .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain a non-alphanumeric character.");
        });

        When(x => x.AuthProvider == PatientAuthProvider.Email, () =>
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(256);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(12)
                .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain a digit.")
                .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain a non-alphanumeric character.");
        });

        When(
            x => x.AuthProvider is PatientAuthProvider.Google or PatientAuthProvider.Apple,
            () =>
            {
                RuleFor(x => x.IdToken)
                    .NotEmpty()
                    .MaximumLength(8192);
            });
    }
}
