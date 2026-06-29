using FluentValidation;

namespace HealthPlatform.Application.PharmacyOrders.RequestMedicationOrderClarification;

public sealed class RequestMedicationOrderClarificationCommandValidator
    : AbstractValidator<RequestMedicationOrderClarificationCommand>
{
    public RequestMedicationOrderClarificationCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Message).NotEmpty().MaximumLength(1000);
    }
}
