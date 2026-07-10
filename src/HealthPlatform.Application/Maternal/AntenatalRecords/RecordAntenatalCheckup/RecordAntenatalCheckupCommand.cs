using HealthPlatform.Application.Maternal.AntenatalRecords.RecordAntenatalCheckup;
using MediatR;

namespace HealthPlatform.Application.Maternal.AntenatalRecords.RecordAntenatalCheckup;

public sealed record RecordAntenatalCheckupCommand(
    Guid AntenatalRecordId,
    Guid? ScheduleEntryId,
    int GestationalAgeWeeks,
    int? FetalHeartRateBpm,
    decimal? FundalHeightCm,
    decimal? EstimatedFetalWeightGrams,
    int? BloodPressureSystolic,
    int? BloodPressureDiastolic,
    decimal? MaternalWeightKg,
    string? ClinicalNotes,
    int? FetalMonitoringReminderIntervalDays) : IRequest<AntenatalCheckupEntryDto>;
