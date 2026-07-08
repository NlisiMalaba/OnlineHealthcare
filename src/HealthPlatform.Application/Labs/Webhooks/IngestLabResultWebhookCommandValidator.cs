using FluentValidation;

namespace HealthPlatform.Application.Labs.Webhooks;

public sealed class IngestLabResultWebhookCommandValidator : AbstractValidator<IngestLabResultWebhookCommand>
{
    public IngestLabResultWebhookCommandValidator()
    {
        RuleFor(x => x.LabPartnerCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.LabPartnerOrderReference).NotEmpty().MaximumLength(128);
        RuleFor(x => x.TestCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(128);
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(260);
        RuleFor(x => x.FileContent).NotEmpty();
    }
}
