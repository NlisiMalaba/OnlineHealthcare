using HealthPlatform.Domain.Maternal;

namespace HealthPlatform.Application.Maternal.BirthPlans;

public static class BirthPlanMappings
{
    public static BirthPlanDto ToDto(this BirthPlan birthPlan) =>
        new(
            birthPlan.Id,
            birthPlan.PatientId,
            birthPlan.AntenatalRecordId,
            birthPlan.Content.ToDto(),
            birthPlan.CreatedAtUtc,
            birthPlan.UpdatedAtUtc);

    public static BirthPlanContentDto ToDto(this BirthPlanContent content) =>
        new(
            content.LabourPreferences,
            content.DeliveryMethod,
            content.PainManagement,
            content.PostnatalCare);

    public static BirthPlanContent ToDomain(this BirthPlanContentDto content) =>
        new(
            content.LabourPreferences,
            content.DeliveryMethod,
            content.PainManagement,
            content.PostnatalCare);

    public static MaternalCareAccessGrantDto ToDto(this MaternalCareAccessGrant grant, string doctorFullName) =>
        new(
            grant.Id,
            grant.AntenatalRecordId,
            grant.DoctorId,
            doctorFullName,
            grant.ShareAntenatalRecord,
            grant.ShareBirthPlan,
            grant.GrantedAtUtc,
            grant.IsActive);
}
