using FluentValidation;

namespace HealthPlatform.Application.Labs.CreatePatientLabOrderRequest;

public sealed class CreatePatientLabOrderRequestCommandValidator : AbstractValidator<CreatePatientLabOrderRequestCommand>
{
    public CreatePatientLabOrderRequestCommandValidator()
    {
        RuleFor(x => x.LabPartnerCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.TestCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.ClinicalNotes).MaximumLength(1_000);
    }
}
