using System.Text.RegularExpressions;
using FluentValidation;

namespace HealthPlatform.Application.NextOfKin.AddNextOfKinContact;

public sealed class AddNextOfKinContactCommandValidator : AbstractValidator<AddNextOfKinContactCommand>
{
    private static readonly Regex E164PhonePattern =
        new(@"^\+[1-9]\d{1,14}$", RegexOptions.Compiled);

    public AddNextOfKinContactCommandValidator()
    {
        RuleFor(command => command.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.Relationship)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.PhoneNumber)
            .NotEmpty()
            .MaximumLength(32)
            .Must(phone => E164PhonePattern.IsMatch(phone))
            .WithMessage("Phone number must be in E.164 format (e.g. +263771234567).");

        When(command => !string.IsNullOrWhiteSpace(command.Email), () =>
        {
            RuleFor(command => command.Email)
                .MaximumLength(320)
                .EmailAddress();
        });
    }
}
