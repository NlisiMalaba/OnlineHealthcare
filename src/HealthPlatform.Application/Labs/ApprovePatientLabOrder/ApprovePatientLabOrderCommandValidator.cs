using FluentValidation;

namespace HealthPlatform.Application.Labs.ApprovePatientLabOrder;

public sealed class ApprovePatientLabOrderCommandValidator : AbstractValidator<ApprovePatientLabOrderCommand>
{
    public ApprovePatientLabOrderCommandValidator()
    {
        RuleFor(x => x.LabOrderId).NotEmpty();
    }
}
