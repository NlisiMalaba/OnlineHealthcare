using FluentValidation;
using HealthPlatform.Application.Payments;

namespace HealthPlatform.Application.Payments.Webhooks;

public sealed class ProcessPaymentWebhookCommandValidator : AbstractValidator<ProcessPaymentWebhookCommand>
{
    public ProcessPaymentWebhookCommandValidator()
    {
        RuleFor(x => x.ProviderName)
            .NotEmpty()
            .Must(PaymentGatewayProviders.All.Contains)
            .WithMessage("Provider is not supported.");

        RuleFor(x => x.RawBody).NotEmpty();
        RuleFor(x => x.Headers).NotNull();
    }
}
