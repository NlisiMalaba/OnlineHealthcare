using HealthPlatform.Application.Labs.AnnotateDiagnosticReport;
using Xunit;

namespace HealthPlatform.Tests.Unit.Labs;

public sealed class AnnotateDiagnosticReportCommandValidatorTests
{
    [Fact]
    public void Validate_requires_target_id_and_note()
    {
        var validator = new AnnotateDiagnosticReportCommandValidator();
        var result = validator.Validate(new AnnotateDiagnosticReportCommand(
            DiagnosticAnnotationTargetType.LabResult,
            Guid.Empty,
            string.Empty));

        Assert.False(result.IsValid);
    }
}
