using FluentValidation;
using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Identity.UpdatePatientProfile;

public sealed class UpdatePatientProfileCommandValidator : AbstractValidator<UpdatePatientProfileCommand>
{
    private static readonly HashSet<string> AllowedPhotoContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

    public UpdatePatientProfileCommandValidator()
    {
        RuleFor(x => x)
            .Must(HasAtLeastOneField)
            .WithMessage("At least one profile field or photo must be provided.");

        When(x => x.FullName is not null, () =>
        {
            RuleFor(x => x.FullName)
                .NotEmpty()
                .MaximumLength(200);
        });

        When(x => x.DateOfBirth.HasValue, () =>
        {
            RuleFor(x => x.DateOfBirth)
                .Must(dob => dob <= DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Date of birth cannot be in the future.");
        });

        When(x => x.BloodType.HasValue, () =>
        {
            RuleFor(x => x.BloodType)
                .IsInEnum();
        });

        When(x => x.KnownAllergies is not null, () =>
        {
            RuleForEach(x => x.KnownAllergies!)
                .NotEmpty()
                .MaximumLength(200);
            RuleFor(x => x.KnownAllergies!)
                .Must(list => list.Count <= 50)
                .WithMessage("A maximum of 50 allergies is allowed.");
        });

        When(x => x.ChronicConditions is not null, () =>
        {
            RuleForEach(x => x.ChronicConditions!)
                .NotEmpty()
                .MaximumLength(200);
            RuleFor(x => x.ChronicConditions!)
                .Must(list => list.Count <= 50)
                .WithMessage("A maximum of 50 chronic conditions is allowed.");
        });

        When(x => x.ProfilePhoto is not null, () =>
        {
            RuleFor(x => x.ProfilePhoto!.ContentType)
                .Must(type => AllowedPhotoContentTypes.Contains(type))
                .WithMessage("Profile photo must be JPEG, PNG, or WebP.");

            RuleFor(x => x.ProfilePhoto!.FileName)
                .NotEmpty()
                .MaximumLength(255);

            RuleFor(x => x.ProfilePhoto!.Content)
                .Must(stream => stream.CanRead)
                .WithMessage("Profile photo content is not readable.");

            RuleFor(x => x.ProfilePhoto!.Length)
                .GreaterThan(0)
                .LessThanOrEqualTo(5 * 1024 * 1024);
        });
    }

    private static bool HasAtLeastOneField(UpdatePatientProfileCommand command) =>
        command.FullName is not null
        || command.DateOfBirth.HasValue
        || command.BloodType.HasValue
        || command.KnownAllergies is not null
        || command.ChronicConditions is not null
        || command.ProfilePhoto is not null;
}
