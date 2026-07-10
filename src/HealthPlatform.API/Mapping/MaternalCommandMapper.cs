using HealthPlatform.API.Requests.Maternal;
using HealthPlatform.Application.Maternal.AntenatalRecords.ConfigureFetalMonitoringReminders;
using HealthPlatform.Application.Maternal.AntenatalRecords.CreateAntenatalRecord;
using HealthPlatform.Application.Maternal.AntenatalRecords.RecordAntenatalCheckup;
using HealthPlatform.Application.Maternal.BirthPlans;
using HealthPlatform.Application.Maternal.BirthPlans.CreateBirthPlan;
using HealthPlatform.Application.Maternal.BirthPlans.GrantMaternalCareAccess;
using HealthPlatform.Application.Maternal.BirthPlans.RevokeMaternalCareAccess;
using HealthPlatform.Application.Maternal.BirthPlans.UpdateBirthPlan;
using HealthPlatform.Application.Maternal.ChildProfiles.CreateChildProfile;
using HealthPlatform.Application.Vaccinations.ListChildVaccinationRecords;
using HealthPlatform.Application.Vaccinations.ListChildVaccinationSchedule;
using HealthPlatform.Application.Vaccinations.RecordChildVaccination;

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

    public static CreateBirthPlanCommand ToCreateBirthPlanCommand(
        Guid antenatalRecordId,
        BirthPlanContentRequest request) =>
        new(antenatalRecordId, ToBirthPlanContentDto(request));

    public static UpdateBirthPlanCommand ToUpdateBirthPlanCommand(
        Guid antenatalRecordId,
        BirthPlanContentRequest request) =>
        new(antenatalRecordId, ToBirthPlanContentDto(request));

    public static GrantMaternalCareAccessCommand ToGrantMaternalCareAccessCommand(
        Guid antenatalRecordId,
        GrantMaternalCareAccessRequest request) =>
        new(
            antenatalRecordId,
            request.DoctorId,
            request.ShareAntenatalRecord,
            request.ShareBirthPlan);

    public static RevokeMaternalCareAccessCommand ToRevokeMaternalCareAccessCommand(
        Guid antenatalRecordId,
        Guid doctorId) =>
        new(antenatalRecordId, doctorId);

    public static CreateChildProfileCommand ToCreateChildProfileCommand(CreateChildProfileRequest request) =>
        new(
            request.FullName,
            request.DateOfBirth,
            request.BloodType,
            request.KnownAllergies);

    public static RecordChildVaccinationCommand ToRecordChildVaccinationCommand(
        Guid childProfileId,
        RecordVaccinationRequest request) =>
        new(
            childProfileId,
            request.ScheduleEntryId,
            request.VaccineName,
            request.AdministeredDate,
            request.BatchNumber,
            request.Provider);

    private static BirthPlanContentDto ToBirthPlanContentDto(BirthPlanContentRequest request) =>
        new(
            request.LabourPreferences,
            request.DeliveryMethod,
            request.PainManagement,
            request.PostnatalCare);
}
