using HealthPlatform.API.Requests.Maternal;
using HealthPlatform.Application.Maternal.AntenatalRecords.ConfigureFetalMonitoringReminders;
using HealthPlatform.Application.Maternal.AntenatalRecords.CreateAntenatalRecord;
using HealthPlatform.Application.Maternal.AntenatalRecords.RecordAntenatalCheckup;

namespace HealthPlatform.API.Mapping;

public static class MaternalCommandMapper
{
    public static CreateAntenatalRecordCommand ToCreateAntenatalRecordCommand(
        CreateAntenatalRecordRequest request) =>
        new(
            request.EstimatedDueDate,
            request.GestationalAgeWeeks,
            request.ObstetricDoctorId);

    public static RecordAntenatalCheckupCommand ToRecordAntenatalCheckupCommand(
        Guid antenatalRecordId,
        RecordAntenatalCheckupRequest request) =>
        new(
            antenatalRecordId,
            request.ScheduleEntryId,
            request.GestationalAgeWeeks,
            request.FetalHeartRateBpm,
            request.FundalHeightCm,
            request.EstimatedFetalWeightGrams,
            request.BloodPressureSystolic,
            request.BloodPressureDiastolic,
            request.MaternalWeightKg,
            request.ClinicalNotes,
            request.FetalMonitoringReminderIntervalDays);

    public static ConfigureFetalMonitoringRemindersCommand ToConfigureFetalMonitoringRemindersCommand(
        Guid antenatalRecordId,
        ConfigureFetalMonitoringRemindersRequest request) =>
        new(antenatalRecordId, request.IntervalDays);
}
