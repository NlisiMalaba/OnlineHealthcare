using FluentValidation;

namespace HealthPlatform.Application.Labs.Webhooks;

public sealed class IngestRadiologyReportWebhookCommandValidator : AbstractValidator<IngestRadiologyReportWebhookCommand>
{
    public IngestRadiologyReportWebhookCommandValidator()
    {
        RuleFor(x => x.LabPartnerCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.LabPartnerOrderReference).NotEmpty().MaximumLength(128);
        RuleFor(x => x.ReportFileContent).NotEmpty();
        RuleFor(x => x.ReportContentType).NotEmpty().MaximumLength(128);
        RuleFor(x => x.ReportFileName).NotEmpty().MaximumLength(260);
        RuleForEach(x => x.ImagingFiles).ChildRules(imaging =>
        {
            imaging.RuleFor(x => x.FileContent).NotEmpty();
            imaging.RuleFor(x => x.ContentType).NotEmpty().MaximumLength(128);
            imaging.RuleFor(x => x.FileName).NotEmpty().MaximumLength(260);
        });
    }
}
