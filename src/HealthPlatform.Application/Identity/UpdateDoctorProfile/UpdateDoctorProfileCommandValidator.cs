using FluentValidation;
using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Identity.UpdateDoctorProfile;

public sealed class UpdateDoctorProfileCommandValidator : AbstractValidator<UpdateDoctorProfileCommand>
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

    public UpdateDoctorProfileCommandValidator()
    {
        RuleFor(x => x)
            .Must(HasAtLeastOneField)
            .WithMessage("At least one profile field, availability slot, photo, or credentials must be provided.");

        When(x => x.VirtualFee.HasValue, () =>
        {
            RuleFor(x => x.VirtualFee)
                .GreaterThanOrEqualTo(0);
        });

        When(x => x.PhysicalFee.HasValue, () =>
        {
            RuleFor(x => x.PhysicalFee)
                .GreaterThanOrEqualTo(0);
        });

        When(x => x.Bio is not null, () =>
        {
            RuleFor(x => x.Bio)
                .MaximumLength(2000);
        });

        When(x => x.AvailabilitySlots is not null, () =>
        {
            RuleFor(x => x.AvailabilitySlots!)
                .NotEmpty()
                .WithMessage("At least one availability slot is required when updating availability.");

            RuleFor(x => x.AvailabilitySlots!)
                .Must(slots => slots.Count <= 50)
                .WithMessage("A maximum of 50 availability slots is allowed.");

            RuleForEach(x => x.AvailabilitySlots!).ChildRules(slot =>
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
        });

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

    private static bool HasAtLeastOneField(UpdateDoctorProfileCommand command) =>
        command.VirtualFee.HasValue
        || command.PhysicalFee.HasValue
        || command.Bio is not null
        || command.AvailabilitySlots is not null
        || command.ProfilePhoto is not null
        || command.Credentials is not null;
}
