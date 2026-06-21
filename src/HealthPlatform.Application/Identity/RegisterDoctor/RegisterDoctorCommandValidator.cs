using FluentValidation;
using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Identity.RegisterDoctor;

public sealed class RegisterDoctorCommandValidator : AbstractValidator<RegisterDoctorCommand>
{
    private static readonly HashSet<string> AllowedPhotoContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

    private static readonly HashSet<string> AllowedCredentialContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "image/jpeg",
            "image/png"
        };

    private static readonly System.Text.RegularExpressions.Regex E164PhonePattern =
        new(@"^\+[1-9]\d{1,14}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public RegisterDoctorCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.LicenseNumber)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.Specialty)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.YearsOfExperience)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(80);

        RuleFor(x => x.ClinicAddress)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x)
            .Must(x => x.ClinicLatitude.HasValue == x.ClinicLongitude.HasValue)
            .WithMessage("Clinic latitude and longitude must both be provided or both omitted.");

        When(x => x.ClinicLatitude.HasValue, () =>
        {
            RuleFor(x => x.ClinicLatitude)
                .InclusiveBetween(-90, 90);

            RuleFor(x => x.ClinicLongitude)
                .InclusiveBetween(-180, 180);
        });

        RuleFor(x => x.VirtualFee)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.PhysicalFee)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Bio)
            .MaximumLength(2000)
            .When(x => x.Bio is not null);

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

        RuleFor(x => x.AvailabilitySlots)
            .NotEmpty()
            .WithMessage("At least one availability slot is required.");

        RuleForEach(x => x.AvailabilitySlots).ChildRules(slot =>
        {
            slot.RuleFor(s => s.DayOfWeek)
                .IsInEnum();

            slot.RuleFor(s => s.AppointmentType)
                .IsInEnum();

            slot.RuleFor(s => s.StartTime)
                .LessThan(s => s.EndTime)
                .WithMessage("Start time must be before end time.");

            slot.RuleFor(s => s.SlotDurationMinutes)
                .GreaterThan(0)
                .LessThanOrEqualTo(240);
        });

        RuleFor(x => x.AvailabilitySlots)
            .Must(slots => slots.Count <= 50)
            .WithMessage("A maximum of 50 availability slots is allowed.");

        When(x => x.ProfilePhoto is not null, () =>
        {
            RuleFor(x => x.ProfilePhoto!.ContentType)
                .Must(type => AllowedPhotoContentTypes.Contains(type))
                .WithMessage("Profile photo must be JPEG, PNG, or WebP.");

            RuleFor(x => x.ProfilePhoto!.FileName)
                .NotEmpty()
                .MaximumLength(255);

            RuleFor(x => x.ProfilePhoto!.Length)
                .GreaterThan(0)
                .LessThanOrEqualTo(5 * 1024 * 1024);
        });

        RuleFor(x => x.Credentials)
            .NotNull()
            .WithMessage("Credentials document is required.");

        When(x => x.Credentials is not null, () =>
        {
            RuleFor(x => x.Credentials!.ContentType)
                .Must(type => AllowedCredentialContentTypes.Contains(type))
                .WithMessage("Credentials must be PDF, JPEG, or PNG.");

            RuleFor(x => x.Credentials!.FileName)
                .NotEmpty()
                .MaximumLength(255);

            RuleFor(x => x.Credentials!.Length)
                .GreaterThan(0)
                .LessThanOrEqualTo(10 * 1024 * 1024);
        });
    }
}
