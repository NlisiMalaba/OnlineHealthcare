namespace HealthPlatform.API.Requests.Referrals;

public sealed class CreateReferralRequest
{
    public Guid PatientId { get; init; }

    public Guid? ReceivingDoctorId { get; init; }

    public string? ReceivingHospitalName { get; init; }

    public string Reason { get; init; } = string.Empty;

    public string? ClinicalNotes { get; init; }

    public IReadOnlyList<string> SharedHealthRecordSections { get; init; } = [];

    public DateTime PatientConsentAtUtc { get; init; }
}
