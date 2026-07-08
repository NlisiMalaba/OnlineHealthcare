using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.HealthRecords.ExportPatientHealthRecordPdf;

public sealed record ExportPatientHealthRecordPdfQuery() : IQuery<HealthRecordPdfExportDto>;
