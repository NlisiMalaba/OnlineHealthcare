using HealthPlatform.Domain.Maternal;

namespace HealthPlatform.Application.Maternal.BirthPlans;

public sealed record BirthPlanContentDto(
    string? LabourPreferences,
    string? DeliveryMethod,
    string? PainManagement,
    string? PostnatalCare);

public sealed record BirthPlanDto(
    Guid Id,
    Guid PatientId,
    Guid AntenatalRecordId,
    BirthPlanContentDto Content,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record MaternalCareAccessGrantDto(
    Guid Id,
    Guid AntenatalRecordId,
    Guid DoctorId,
    string DoctorFullName,
    bool ShareAntenatalRecord,
    bool ShareBirthPlan,
    DateTime GrantedAtUtc,
    bool IsActive);
