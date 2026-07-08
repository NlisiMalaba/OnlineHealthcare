namespace HealthPlatform.Application.Referrals;

public sealed record ReferralDto(
    Guid Id,
    Guid PatientId,
    Guid ReferringDoctorId,
    Guid? ReceivingDoctorId,
    string? ReceivingHospitalName,
    string Reason,
    string? ClinicalNotes,
    IReadOnlyList<string> SharedHealthRecordSections,
    DateTime PatientConsentAtUtc,
    string Status,
    string? ResponseReason,
    DateTime? RespondedAtUtc,
    string? ConsultationSummaryEntryId,
    DateTime CreatedAtUtc);
