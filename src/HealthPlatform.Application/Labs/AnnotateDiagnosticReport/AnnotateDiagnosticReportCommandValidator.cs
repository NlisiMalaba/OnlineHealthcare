using FluentValidation;

namespace HealthPlatform.Application.Labs.AnnotateDiagnosticReport;

public sealed class AnnotateDiagnosticReportCommandValidator : AbstractValidator<AnnotateDiagnosticReportCommand>
{
    public AnnotateDiagnosticReportCommandValidator()
    {
        RuleFor(x => x.TargetType).IsInEnum();
        RuleFor(x => x.TargetId).NotEmpty();
        RuleFor(x => x.Note).NotEmpty().MaximumLength(2_000);
    }
}
