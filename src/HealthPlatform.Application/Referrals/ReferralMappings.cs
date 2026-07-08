using HealthPlatform.Domain.Referrals;

namespace HealthPlatform.Application.Referrals;

public static class ReferralMappings
{
    public static ReferralDto ToDto(this Referral referral) =>
        new(
            referral.Id,
            referral.PatientId,
            referral.ReferringDoctorId,
            referral.ReceivingDoctorId,
            referral.ReceivingHospitalName,
            referral.Reason,
            referral.ClinicalNotes,
            referral.SharedHealthRecordSections,
            referral.PatientConsentAtUtc,
            referral.Status.ToString().ToLowerInvariant(),
            referral.CreatedAtUtc);
}
