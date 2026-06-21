using FluentValidation;

namespace HealthPlatform.Application.Identity.RegisterPharmacy;

public sealed class RegisterPharmacyCommandValidator : AbstractValidator<RegisterPharmacyCommand>
{
    private static readonly HashSet<string> AllowedLogoContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

    private static readonly System.Text.RegularExpressions.Regex E164PhonePattern =
        new(@"^\+[1-9]\d{1,14}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public RegisterPharmacyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Must(phone => E164PhonePattern.IsMatch(phone))
            .WithMessage("Phone number must be in E.164 format (e.g. +263771234567).");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(12)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain a non-alphanumeric character.");

        When(x => x.Latitude.HasValue || x.Longitude.HasValue, () =>
        {
            RuleFor(x => x.Latitude)
                .NotNull()
                .InclusiveBetween(-90, 90);

            RuleFor(x => x.Longitude)
                .NotNull()
                .InclusiveBetween(-180, 180);
        });

        When(x => x.Logo is not null, () =>
        {
            RuleFor(x => x.Logo!.ContentType)
                .Must(type => AllowedLogoContentTypes.Contains(type))
                .WithMessage("Logo must be JPEG, PNG, or WebP.");

            RuleFor(x => x.Logo!.FileName)
                .NotEmpty()
                .MaximumLength(255);

            RuleFor(x => x.Logo!.Length)
                .GreaterThan(0)
                .LessThanOrEqualTo(5 * 1024 * 1024);
        });
    }
}
