using HealthPlatform.API.Requests.Referrals;
using HealthPlatform.Application.Referrals.CreateReferral;
using HealthPlatform.Application.Referrals.RespondToReferral;

namespace HealthPlatform.API.Mapping;

public static class ReferralCommandMapper
{
    public static CreateReferralCommand ToCreateCommand(CreateReferralRequest request) =>
        new(
            request.PatientId,
            request.ReceivingDoctorId,
            request.ReceivingHospitalName,
            request.Reason,
            request.ClinicalNotes,
            request.SharedHealthRecordSections,
            request.PatientConsentAtUtc);

    public static RespondToReferralCommand ToRespondCommand(Guid referralId, RespondToReferralRequest request) =>
        new(
            referralId,
            request.Action,
            request.Reason);
}
