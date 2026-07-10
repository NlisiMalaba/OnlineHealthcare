using FluentValidation;
using HealthPlatform.Application.Maternal.ChildProfiles;

namespace HealthPlatform.Application.Maternal.ChildProfiles.CreateChildProfile;

public sealed class CreateChildProfileCommandValidator : AbstractValidator<CreateChildProfileCommand>
{
    public CreateChildProfileCommandValidator()
    {
        RuleFor(command => command.FullName)
            .NotEmpty()
            .MaximumLength(ChildProfilePolicies.MaxFullNameLength);

        RuleFor(command => command.DateOfBirth)
            .NotEmpty();

        RuleFor(command => command.BloodType)
            .MaximumLength(ChildProfilePolicies.MaxBloodTypeLength)
            .When(command => !string.IsNullOrWhiteSpace(command.BloodType));

        RuleFor(command => command.KnownAllergies)
            .NotNull()
            .Must(allergies => allergies.Count <= ChildProfilePolicies.MaxKnownAllergies)
            .WithMessage($"At most {ChildProfilePolicies.MaxKnownAllergies} known allergies are allowed.");

        RuleForEach(command => command.KnownAllergies)
            .MaximumLength(ChildProfilePolicies.MaxAllergyLength)
            .When(command => command.KnownAllergies is not null);
    }
}
