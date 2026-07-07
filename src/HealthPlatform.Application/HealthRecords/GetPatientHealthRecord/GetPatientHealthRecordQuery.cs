using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.HealthRecords.GetPatientHealthRecord;

public sealed record GetPatientHealthRecordQuery() : IQuery<PatientHealthRecordDto>;
