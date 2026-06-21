using FluentValidation;

namespace HealthPlatform.Application.Identity.UpdatePharmacyProfile;

public sealed class UpdatePharmacyProfileCommandValidator : AbstractValidator<UpdatePharmacyProfileCommand>
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

    public UpdatePharmacyProfileCommandValidator()
    {
        RuleFor(x => x)
            .Must(HasAtLeastOneField)
            .WithMessage("At least one profile field or logo must be provided.");

        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);
        });

        When(x => x.Address is not null, () =>
        {
            RuleFor(x => x.Address)
                .NotEmpty()
                .MaximumLength(500);
        });

        When(x => x.Latitude.HasValue || x.Longitude.HasValue, () =>
        {
            RuleFor(x => x.Latitude)
                .NotNull()
                .InclusiveBetween(-90, 90);

            RuleFor(x => x.Longitude)
                .NotNull()
                .InclusiveBetween(-180, 180);
        });

        When(x => x.PhoneNumber is not null, () =>
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .Must(phone => E164PhonePattern.IsMatch(phone!))
                .WithMessage("Phone number must be in E.164 format (e.g. +263771234567).");
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

    private static bool HasAtLeastOneField(UpdatePharmacyProfileCommand command) =>
        command.Name is not null
        || command.Address is not null
        || command.Latitude.HasValue
        || command.Longitude.HasValue
        || command.PhoneNumber is not null
        || command.Logo is not null;
}
