using FluentValidation;

namespace HealthPlatform.Application.Insurance.Webhooks;

public sealed class ProcessInsuranceClaimWebhookCommandValidator
    : AbstractValidator<ProcessInsuranceClaimWebhookCommand>
{
    public ProcessInsuranceClaimWebhookCommandValidator()
    {
        RuleFor(x => x.InsurerCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.RawBody).NotEmpty();
        RuleFor(x => x.Headers).NotNull();
    }
}
