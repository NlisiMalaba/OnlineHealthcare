using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Referrals.CreateReferral;

public sealed record CreateReferralCommand(
    Guid PatientId,
    Guid? ReceivingDoctorId,
    string? ReceivingHospitalName,
    string Reason,
    string? ClinicalNotes,
    IReadOnlyList<string> SharedHealthRecordSections,
    DateTime PatientConsentAtUtc) : ICommand<ReferralDto>;
