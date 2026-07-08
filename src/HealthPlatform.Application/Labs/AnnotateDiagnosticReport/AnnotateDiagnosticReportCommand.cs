using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Labs.AnnotateDiagnosticReport;

public enum DiagnosticAnnotationTargetType
{
    LabResult = 0,
    RadiologyReport = 1
}

public sealed record AnnotateDiagnosticReportCommand(
    DiagnosticAnnotationTargetType TargetType,
    Guid TargetId,
    string Note) : ICommand<HealthRecords.HealthRecordEntryDto>;
