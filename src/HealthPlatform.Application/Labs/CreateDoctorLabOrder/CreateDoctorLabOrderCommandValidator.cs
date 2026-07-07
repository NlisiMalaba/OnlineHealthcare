using FluentValidation;

namespace HealthPlatform.Application.Labs.CreateDoctorLabOrder;

public sealed class CreateDoctorLabOrderCommandValidator : AbstractValidator<CreateDoctorLabOrderCommand>
{
    public CreateDoctorLabOrderCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.HealthRecordId).NotEmpty();
        RuleFor(x => x.LabPartnerCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.TestCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.ClinicalNotes).MaximumLength(1_000);
    }
}
